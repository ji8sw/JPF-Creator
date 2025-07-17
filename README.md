# JPF Creator
JPF (Ji9sw Package File) is a custom file format made to store a bundle of files to be read by a game engine

JPF is **not** something to be used by a user, and is instead made for game and engine developers to implement into their engines

## How To Use
To use the JPF Creator simply download the latest release (or build yourself), then run JPF Creator.exe

You can then press Add Files to add game assets that you want in the JPF, then press Compile and save the JPF wherever.

To use the JPF files read "C++ JPF Reader"

## JPF File Format

JPF files can be identified using the first 3 bytes of a `.jpf` file, the first 3 bytes should always be a magic header reading `JPF`

After the magic header, the first few bytes will be:
- 8 bytes for file name hash (CityHash64)
- 1 byte for file type
- 4 bytes for file length
- [FILE_LENGTH] bytes for file data
- next file...

## C++ JPF Reader

A C++ program has been developed which can easily read JPF files that are created by this application
It is very easy to implement in your own projects.

[View Source Code](https://github.com/ji8sw/JPF-Reader)

## To-Do
- [ ] Add optional file compression
- [ ] Add optional file encryption
- [ ] Click selected files to view more info or remove specific files
- [ ] Beautify Interface
- [x] C++ JPF Reader