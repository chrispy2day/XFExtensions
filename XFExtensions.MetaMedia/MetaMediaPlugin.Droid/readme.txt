If your app should only be installed on devices that have a camera then add the Camera permission to the manifest.
However, the app will still work without this permission because we are using an Intent for the camera.

Requires WriteExternalStorage and either AccessFineLocation or AccessCoarseLocation (fine automatically gives coarse so no need for both).