# LocalPreferences
Better alternative for Unity's PlayerPrefs

![LocalPrefsWindow](https://i.imgur.com/y7fWUQJ.png)\
Made in Unity 2019.3, but should work with older versions.

## Features
- Save your game or editor preferences in JSON files
- Edit data files in editor window

## Performance

| Set | 1000 floats | 10000 floats |
| :---         |     :---:      |     :---:      |
| LocalPrefs   | 3-8ms     | 100ms    |
| PlayerPrefs     | 40-45ms       | 6,300ms      |

### Note: Doesn't tested on Android.

## TODO
- Rijndael Encryption
- Guideline on implementing custom types
- Support for more types (Texture as byte array, Matrix4x4, etc)
