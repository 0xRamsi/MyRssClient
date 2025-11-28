This project is my attempt to create an RSS-Reader that feels a little like Instagram or Tiktok, but isn't social.

In it's core it is just an RSS-Reader, but it displays posts a nicely. It also gives you the ability to like a specific Post (an RSS-Item), and that information will be used to rank the posts you see next time you open the app.
The challenge is that the entire thing is local. No server will record your likes, or what you click and don't click.

Of course, a big drawback is that you have no 'friends' or 'followers', so no one will see what you like here. I acknowledge that this is a major driver for many people to use a social platform, but this is not my goal. My goal is to have an RSS-Reader that *feels like* social media.

Current state: The basic setup is there, so it can be used as a RSS-Reader. The main point of creating a ranking algorithm that works locally, is the next step.

Feel free to send PRs with implementations of ranking algorithms.

Note: This is more a proof of concept, and written in C#, cause this is the language I know best. If you actually want a working app on your phone, I suggest rewriting in a different language (Kotlin for Android, Javascript for an Firefox extension etc.). Just take the idea and you possibly can use the DB-Schema as a good basis to start from.