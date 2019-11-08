# LocalPreferences
Better alternative for Unity's PlayerPrefs
#### Edit > Local Preferences > Player / Editor

![LocalPrefsWindow](https://i.imgur.com/WWsObkL.png)\
Made in Unity 2019.3, but should work with older versions.

## Features
- Save your Game or Editor preferences in JSON files
- Encrypt game data with Advanced Encryption Standard
- Manage data files in Editor Window (Playmode Supported)
- Auto Save on player quit
- Slighly faster than PlayerPrefs (even with enabled Encryption)

## Performance

| Set | 1000 floats | 10000 floats |
| :---         |     :---:      |     :---:      |
| LocalPrefs   | 3-6ms     | 30-50ms    |
| PlayerPrefs     | 40-45ms       | 6,300ms      |

| Get | 1000 floats | 10000 floats |
| :---         |     :---:      |     :---:      |
| LocalPrefs   | 4-6ms     | 32-40ms    |
| PlayerPrefs     | 2ms       | 67ms      |

| Encryption | 1000 floats | 10000 floats |
| :---         |     :---:      |     :---:      |
| Load   | 40-60ms     | 40-60ms    |
| Save     | 40-60ms       | 40-60ms      |

### Note: Not tested on Linux, Android.

## TODO
- ~~Rijndael Encryption~~
- Optimized scroll view
- Support for more types (Texture as byte array, Matrix4x4, etc)
- Delegate on preference changes
- Default save file
- Advanced search filter
- Reordering
- Documentation
- Guideline on implementing custom types
- Make asset as package
- Get Async?
