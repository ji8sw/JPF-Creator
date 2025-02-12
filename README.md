# JPF Creator
JPF (Ji9sw Package File) is a custom file format made to store a bundle of files to be read by a game engine

JPF is **not** something to be used by a user, and is instead made for game and engine developers to implement into their engines

## JPF File Format

JPF files can be identified using the first 3 bytes of a `.jpf` file, the first 3 bytes should always be a magic header reading `JPF`

After the magic header, the first few bytes will be:
- 4 bytes for file name hash (CityHash32)
- 1 byte for file type
- 4 bytes for file length
- [FILE_LENGTH] bytes for file data
- next file...

## To-Do
- [ ] Add optional file compression
- [ ] Add optional file encryption
- [ ] Click selected files to view more info or remove specific files
- [ ] Beautify Interface
- [ ] C++ JPF Reader