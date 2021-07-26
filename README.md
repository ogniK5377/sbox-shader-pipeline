# sbox-shader pipeline

![Example](https://i.imgur.com/zTQWg1Y.gif)

This is a extremely minimal shader setup to create shaders in s&box. Currently this is not an OFFICIAL way to setup shaders but it works. Everything here is subject to change and will be deprecated in the future when official support is added. All function names and functionality was grabbed from reverse engineering and there could be still plenty of differences on how it works or how it's setup.

# Usage
Load up the gamemode and check out the example of `example_shader.vfx` in `shaders/example_shader.vfx`. If you want to access the properties of the shader, open up the material `materials/example_shader_material.vmat`.

In game two console commands are provided, `shaderpl_cube` which will spawn a cube with the `materials/example_shader_material.vmat` material attached to it and `shaderpl_plane` which will do the same as above except with a plane.

# TODO
Break out textures into a nice reasonable way, technically with the current setup you are able to use textures it's just not nicely abstracted away yet. There's a few issues with lighting and normal unpacking too due to `g_tTransformTexture` being funky so that also needs to be looked at too.

# Footnote
## Dynamic lights
If you want to access world lights you can look into the `re_Cb5.fxc` file which contains all lighting information based on the dynamic lights in the scene.


## Skyboxes
If you plan on creating a custom skybox which is used with the SkyObject, you must use the PerViewConstantBufferSky variant as the register locations end up being different.

## Material editor refreshing
If you want to refresh your shader within the material editor to update the properties with the click of the button there's a few things which need to be done.
First your shader must be defined as a "Dev" shader, this property is set within your shaders `HEADER` with the field `DevShader = true;`
Secondly when you create your material you will find it if you press the `Show Dev Shaders` checkbox!
![Show Dev Shaders](https://i.imgur.com/2TbZ6xg.png)
Once done, you will see this refresh button which will cause the material editor to refresh your shader to update any missing properties and what not!
![Refresh button in the material editor](https://i.imgur.com/hmJMWDE.png)

## Crashes
There's still quite a few bugs with shaders in regards to how s&box deals with them. One common crash you might experience is creating an empty `.vfx` file, if this happens just make sure you have text within the file and reopen the game! Any other crashes you have are most likely caused from incorrectly parsing attributes on objects.