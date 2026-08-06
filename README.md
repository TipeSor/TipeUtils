# TipeUtils

My personal source-only C# utils.

The point is to reference one package/project and let the build pull in the utility sources that make sense for the current project.

For example, common console helpers are always available. Raylib helpers are added only when the project already references Raylib. Future folders can depend on packages, feature flags, or both.

That way I do not have to keep track of a pile of tiny utility NuGets. I can keep the code here, add presets as needed, and use the same TipeUtils reference everywhere.

By default, injected sources are compiled in the `TipeUtils` namespace. A consuming project can compile them into the global namespace instead:

```xml
<PropertyGroup>
  <TipeUtilsNamespace>false</TipeUtilsNamespace>
</PropertyGroup>
```
