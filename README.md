# UIWidgets.ScrollView
> 2D ScrollView for UIWidgets

# How to add
Add this line to your package.json dependencies list
```
"com.justzht.uiwidgets.scrollview": "https://github.com/JustinFincher/UIWidgets.ScrollView.git"
```

# Usage
```csharp
new ScrollView
(
    Widget child,  // child, usually a container
    float minScale = 0.5f,  // minimal scale value
    float maxScale = 3.0f,  
    float contentSizeWidth = 2000,  // content size
    float contentSizeHeight = 2000,  // content size
    Key key = null
);
```

# Notice
This package have a dependency called 'com.justzht.uiwidgets.helper', which you can find on [UIWidgets.Helpers](https://github.com/JustinFincher/UIWidgets.Helpers). 