# notepad#

A text editor that edits text. That's it. That's the app.

## What happened to Notepad?

Microsoft's Notepad — once the platonic ideal of "just open a file and type" — now ships with AI integration, cloud sync, and as of recently, a [remote code execution vulnerability](https://msrc.microsoft.com/update-guide/vulnerability/CVE-2024-49019). A text editor. With an RCE. Let that sink in.

Somewhere along the way, Notepad forgot that it was Notepad.

## What notepad# does

- Opens text files
- Saves text files
- Finds text in your text files
- Prints your text files (yes, all the pages)
- Lets you pick a font
- Lets you zoom in and out

## What notepad# does NOT do

- **Does not render text.** It's plain text. You get what you type.
- **Does not format text.** No bold. No headers. No "smart" quotes. Just characters.
- **Does not connect to the internet.** There is no reason for a text editor to connect to the internet.
- **Does not have opinions about your writing.** No AI suggestions. No Copilot. No "did you mean...?" You are free to write poorly in peace.
- **Does not have a remote code execution vulnerability.** The bar is on the floor and we clear it.

## Building

```
dotnet build
```

## Requirements

- .NET 8.0
- Windows (WinForms)
- A desire to just edit some text without your text editor phoning home

## About

Created by adelon @ adelon.studio
