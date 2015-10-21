# Extensions for Xamarin.Forms
XamarinForms Extensions (XFExtensions for short) is a project to add additional controls and services to make it faster and easier to develop great apps in Xamarin.Forms.

## Control Extensions for Xamarin.Forms
These custom controls make it faster and easier to create great apps using Xamarin.Forms.  You can install the controls via [NuGet](https://www.nuget.org/packages/XamarinFormsExtensions.Controls/).

	PM> Install-Package XamarinFormsExtensions.Controls

The controls currently included are:
            
###SimpleList: 
For small lists, the UI of the built-in ListView leaves a lot to be desired.  You can use a StackLayout, but then you lose the ability to have an ItemTemplate and ItemSelectedCommand.  That's what SimpleList was made for - it gives you the UI control of the StackLayout with the functionality of a ListView.  SimpleList is not nearly as performant as ListView, so please don't use it for large lists of data!
Works On: Android, iOS, WinPhone
            
###GestureView: 
The gesture view control allows you to receive all those great native gestures like swipes, pinches, etc.  Just add the gesture view control as the container for whatever elements should recognize the gesture.
Note: Xamarin.Forms has had some bugs in the past with gestures being swallowed by controls and not reaching the gesture view. This depends on the version and controls that you are using, but if you experience problems, please check your Xamarin.Forms version.  The sample project on GitHub is currently using 1.5.0.6447 successfully.
Works On: Android, iOS

###ZoomImage:  
Pinch and zoom photos as well as double tap to zoom.  Control allows you to enable / disable scrolling, zooming, and control the scaling amounts.  As easy to use as the built-in Xamarin.Forms ImageView with even more capabilities!
Works On: Android, iOS

## MetaMedia Extensions for Xamarin.Forms
The MetaMedia extension makes it easy to take or choose images from your Android or iOS device.  Photos taken will contain any metadata, such as exif data, that the source provides along with the current GPS position. The taken photos will be stored in the native camera / image gallery.  You can get a reference to the full file or a scaled image for previewing on the device (Android only at this time).
Works On: Android, iOS

You can install the MetaMedia Extensions from [NuGet](https://www.nuget.org/packages/XamarinFormsExtensions.MetaMedia/).

	PM> Install-Package XamarinFormsExtensions.MetaMedia