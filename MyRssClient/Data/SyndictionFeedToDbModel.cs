using HtmlAgilityPack;
using MyRssClient.Models;
using System.ServiceModel.Syndication;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace MyRssClient.Data {
    public class SyndictionFeedToDbModel : IConverterService<SyndicationFeed, Channel, Post> {
        private static readonly HttpClient client = new();

        public async Task<Channel> ConvertAsync(SyndicationFeed input, Uri rssLink) {
            Channel result = new() {
                Id = Guid.NewGuid(),
                Title = input.Title.Text,
                RssLink = rssLink,
                URLs = [.. input.Links.Select(link => link.Uri).Distinct()],
                Description = input.Description.Text,
                Language = input.Language,
                LastPublishDate = input.LastUpdatedTime,
                LastFetchAt = DateTimeOffset.Now,
            };
            result.Images = await GetAllImagesFromFeed(input, result);
            if (result.Images.Count != 0) {
                result.Images.First().IsSelected = true;
            }
            result.Items = await ConvertItems(input, result);
            return result;
        }

        private async Task<ICollection<ChannelImage>> GetAllImagesFromFeed(SyndicationFeed input, Channel channel) {
            var relPaths = new List<string>() { "favicon.png", "favicon.ico" };
            var paths = relPaths
                .Select(relPath => {
                    Uri? baseUri = input.BaseUri ?? input.Links.FirstOrDefault(l => l.Uri.IsAbsoluteUri)?.Uri;
                    return (baseUri != null)
                        ? new Uri(baseUri, relPath)
                        : null
                    ;
                })
                .OfType<Uri>()
            ;
            if (input.ImageUrl is not null) {
                paths = paths.Prepend(input.ImageUrl);
            }
            var allPaths = input.Links.Where(IsImage).Select(link => link.Uri).Concat(paths)
                .ToList();

            List<ChannelImage> res = [];
            foreach (var path in allPaths) {
                var image = await GetMyImage(path);
                if (image is not null) {
                    res.Add(new ChannelImage() { Id = Guid.NewGuid().ToString(), Path = path, ImageBytes = image, Channel = channel });
                }
            }
            return res;
        }

        private static bool IsImage(SyndicationLink link) {
            // Just a pre-filter, so we can download less data, which GetImageFormat will filter.
            if (link.MediaType != null && link.MediaType.StartsWith("image")) {
                // MIME-TYPE image/png, image/jpg, etc.
                return true;
            }
            return IsImage(link.Uri.ToString());
        }
        private static bool IsImage(string link) {
            var imageEndings = new List<string>() { "png", "jpg", "jpeg", "gif", "bmp", "tiff" };
            if (imageEndings.Any(imageEnding => link.Contains(imageEnding))) {
                return true;
            }

            return false;
        }

        public async Task<ICollection<Post>> ConvertItems(SyndicationFeed feed, Channel parentChannel) {
            ICollection<Post> result = [];
            HtmlDocument SummaryHtmlDoc = new();
            foreach (var item in feed.Items) {
                SummaryHtmlDoc.LoadHtml(item.Summary.Text);
                var post = new Post() {
                    Guid = Guid.NewGuid(),
                    PostIdFromFeed = string.IsNullOrWhiteSpace(item.Id) ? string.Empty : item.Id,
                    Title = item.Title.Text,
                    Description = SummaryHtmlDoc.DocumentNode.InnerText,
                    Links = item.Links.Where(link => !IsImage(link)).Select(link => new Link() { Title = link.Title, Uri = link.Uri }),
                    Authors = JsonSerializer.Serialize(item.Authors),
                    Images = [],
                    PublishDate = item.PublishDate,
                    FetchedAt = DateTimeOffset.Now,
                    IsRead = false,
                    IsLiked = false,
                    ClickCounter = 0,
                    ParentChannel = parentChannel,
                };

                IList<SyndicationLink> potentialImages = TerribleFunctionToGetImagePaths(item);

                // Download and save the images:
                post.Images = [.. (await Task.WhenAll(
                    potentialImages
                        .Select(link => GetMyImageWithPath(link.Uri))))
                        .Where(p => p.Item2 is not null)    // actual null filter.
                        .OfType<(Uri, byte[])>()            // Compiler
                        .Select(image => new PostImage() { Id = Guid.NewGuid().ToString(), Post = post, ImageBytes = image.Item2, Path = image.Item1 })];
                if (post.Images.Count != 0) {
                    post.Images.First().IsSelected = true;
                }
                result.Add(post);
            }
            return result;
        }

        private static List<SyndicationLink> TerribleFunctionToGetImagePaths(SyndicationItem item) {
            /* This function goes through all sorts of ways, because different feeds have different outputs. */

            // Get images from the links and the description.
            var potentialImages = item.Links.Where(IsImage).ToList();
            potentialImages.AddRange(GetSrcOfImgTag(item.Summary.Text));

            // Search around the XML Attributes and <img>-Tags and such.
            foreach (var extension in item.ElementExtensions) {
                var obj = extension.GetObject<XElement>();
                potentialImages.AddRange(obj.Attributes().Select(a => a.Value).Where(IsImage).Select(link => new SyndicationLink(new Uri(link)))); // für BBC.
                potentialImages.AddRange(GetSrcOfImgTag(extension.GetObject<string>()));    // für die tagesschau
            }
            return potentialImages;
        }

        private static IEnumerable<SyndicationLink> GetSrcOfImgTag(string c) {
            var doc = new HtmlDocument();
            doc.LoadHtml(c);
            var anchorNodes = doc.DocumentNode.SelectNodes("//img[@src]");
            if (anchorNodes != null) {
                foreach (var node in anchorNodes) {
                    string hrefValue = node.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrEmpty(hrefValue)) {
                        yield return new SyndicationLink(new Uri(hrefValue));
                    }
                }
            }
        }

        public async Task<(Uri, byte[]?)> GetMyImageWithPath(Uri path) {
            return (path, await GetMyImage(path));
        }
        public async Task<byte[]?> GetMyImage(Uri path) {
            return await GetImage(path);
        }

        private static async Task<byte[]?> GetImage(Uri path) {
            using HttpRequestMessage request = new(HttpMethod.Get, path);
            using HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode) {
                // TODO: https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
                var bytes = await response.Content.ReadAsByteArrayAsync();
                if (GetImageFormat(bytes) == ImageFormat.unknown) {
                    return null;
                } else {
                    return bytes;
                }
            }
            return null;
        }

        public enum ImageFormat {
            bmp,
            jpeg,
            gif,
            tiff,
            png,
            webp,
            unknown
        }

        public static ImageFormat GetImageFormat(byte[] bytes) {
            byte[] bmp = Encoding.ASCII.GetBytes("BM");
            byte[] gif = Encoding.ASCII.GetBytes("GIF");
            byte[] webp1 = Encoding.ASCII.GetBytes("RIFF");
            byte[] webp2 = Encoding.ASCII.GetBytes("WEBP");
            byte[] png = [137, 80, 78, 71];
            byte[] tiff = [73, 73, 42];
            byte[] tiff2 = [77, 77, 42];
            byte[] jpeg = [255, 216, 255, 224];
            byte[] jpeg2 = [255, 216, 255, 225];

            if (webp1.SequenceEqual(bytes.Take(webp1.Length)))
                return ImageFormat.webp;

            if (webp2.SequenceEqual(bytes.Take(webp2.Length)))
                return ImageFormat.webp;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.jpeg;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.png;

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.gif;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.tiff;

            return ImageFormat.unknown;
        }

        private class Atom10Constants {
            public const string HtmlType = "html";
            public const string XHtmlType = "xhtml";
            public const string PlaintextType = "text";
        }
    }
}
