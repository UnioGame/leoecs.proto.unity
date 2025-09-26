<p align="center">
    <img src="./logo.png" alt="Logo">
</p>

# Unity Editor Integration for LeoECS Proto
Unity editor integration with world state monitoring and a runtime for integrating Unity extensions.

- IMPORTANT! Requires C#9 (or Unity >= 2021.2).
- IMPORTANT! Depends on: Leopotam.EcsProto, Leopotam.EcsProto.QoL.
- IMPORTANT! Use DEBUG builds for development and RELEASE builds for production: all internal checks/exceptions work only in DEBUG and are stripped in RELEASE for performance.
- IMPORTANT! Tested on Unity 2021.3 (depends on it) and includes asmdef definitions for separate assembly compilation to reduce main project recompilation time.

# Social
Official blog: https://leopotam.ru

# Installation

## As a Unity package
Install via a git URL in Package Manager or by editing Packages/manifest.json:
```
"ru.leopotam.ecsproto.unity": "https://gitverse.ru/leopotam/ecsproto.unity.git",
```

## As sources
You can clone the code or download an archive from the releases page.

## Other sources
The official working version is hosted at https://gitverse.ru/leopotam/ecsproto.unity. All other versions (including nuget, npm, and other repositories) are unofficial clones or third-party code with unknown contents.

# Editor code templates
The extension adds a Create / LeoECS Proto submenu to the project context menu where you can generate starter code without modules, with modules, and generate an empty module.

# Module setup
The module injects named GameObjects into systems, provides access to worlds for other Unity extensions, and integrates world/system monitoring in the editor:
```c#
// Environment initialization.
using Leopotam.EcsProto.Unity;

IProtoSystems _systems;

void Start () {        
    _systems = new ProtoSystems (new ProtoWorld (new Aspect1 ()));
    _systems
        // Attach the integration module.
        .AddModule (new UnityModule ())
        .Add (new TestSystem1 ())
        .Init ();
}
```

- IMPORTANT! Worlds that should be monitored must be added before the module.
- IMPORTANT! By default component names are baked into the GameObject name whenever the component list changes on an entity. If you don’t need this (e.g., to improve editor performance), change it via NoBakeComponentsInName = true in the UnityModule constructor options.

After attaching the module all worlds are available anywhere via ProtoUnityWorlds:
```c#
// Get the default world.
ProtoWorld defaultWorld = ProtoUnityWorlds.Get ();
// Get a world named "events".
ProtoWorld eventsWorld = ProtoUnityWorlds.Get ("events");
```

All scene objects with a ProtoUnityLink component are available anywhere via ProtoUnityLinks:
```c#
(GameObject go, bool ok) = ProtoUnityLinks.Get (linkName);
if (ok) {
    // The object exists and is ready to use.
}
```

ProtoUnityLink objects can be injected into system fields via a special attribute:
```c#
class TestSystem : IProtoRunSystem {
    [DIUnity ("button1")] readonly GameObject _clickBtn = default;
    [DIUnity ("button1")] readonly Transform _clickBtnTransform = default;
    [DIUnity ("button1")] readonly Button _clickBtnButton = default;
    
    public void Run () {
        // All fields are initialized at this point
        // with references to the same GameObject and its components.
    }
}
```

- IMPORTANT! Worlds are unregistered from global access automatically on IProtoSystems.Destroy().
- IMPORTANT! You can generate a starter class from the Unity editor context menu.

# Visual entity authoring
You can assemble entities via the inspector from prepared components. A component is considered prepared if it is serializable and marked with a special attribute:
```c#
[Serializable, ProtoUnityAuthoring]
struct C1 {
    public Vector3 Position;
}
```

Then add a ProtoUnityAuthoring component to a GameObject in the scene and build by selecting components from the menu.

- IMPORTANT! The menu hierarchy is determined automatically based on the full component type names (namespace, type name).

If you need a custom menu hierarchy, specify it via the attribute:
```c#
[Serializable, ProtoUnityAuthoring ("Game/Combat/Units/Position")]
struct Position : IProtoUnityAuthoring {
    public Vector3 Position;
}
```

Sometimes you need to access the created entity or the GameObject with authoring — implement a special interface:
```c#
[Serializable, ProtoUnityAuthoring]
struct C1 : IProtoUnityAuthoring {
    public ProtoPackedEntityWithWorld Entity;
    
    // Get the entity and the GameObject with authoring.
    public void Authoring (in ProtoPackedEntityWithWorld entity, GameObject go) {
        Entity = entity;
    }
}
```

# Custom events with GameObject
Sometimes ECS events originate only in MonoBehaviours (physics, uGUI, etc.). You may also need to pass a MonoBehaviour as part of an ECS event. Implement a special base class:
```c#
// ECS event that carries our sender.
struct MyTriggerEnterEvent {
    public MyTriggerEnterAction Sender;
}

class MyTriggerEnterAction : ProtoUnityAction<MyTriggerEnterEvent> {
    // Component-specific data.
    public int CustomId = 123;
    
    public void OnTriggerEnter (Collider c) {
        // Check if it's valid to send an event.
        if (IsValidForEvent ()) {
            // Create an event.
            ref MyTriggerEnterEvent msg = ref NewEvent ();
            // Fill fields.
            msg.Sender = this;
        }
    }
}
```
Attach MyTriggerEnterAction to a GameObject and configure its fields. When the trigger-enter event fires, this component will create an ECS event that can be handled by ECS systems.

# Gizmos support
Gizmos.XXX calls are usually made from OnDrawGizmos() inside a MonoBehaviour. The integration module provides a built-in way to call these from systems by implementing an interface:
```c#
class TestGizmoSystem : IProtoRunSystem, IProtoUnityGizmoSystem {
    public void Run () {
        // Process data.
    }

    public void DrawGizmos () {
        // Prefer wrapping editor-only code
        // so it’s stripped from builds.
#if UNITY_EDITOR
        // Same API as in OnDrawGizmos().
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere (new Vector3 (1f, 2f, 3f), 1f);
#endif
    }
}
```

# License
Released under the MIT-ZARYA license, see ./LICENSE.md for details.

# FAQ

### I integrated it but can’t find where to view the world state.
Enter Play Mode and expand the hierarchy under the system scene DontDestroyOnLoad — you should see [PROTO-WORLD] objects for each world. The entire entity hierarchy under these objects is virtual, created only for visualizing component presence on entities and not used by the core.

### I have many entities and want to filter only those with specific components. How?
By default, component type names are appended to an entity’s GameObject name. Use Unity’s hierarchy search bar to filter GameObjects that match a pattern.

### I have multiple worlds and don’t want to see one or more in the scene. How?
By default all connected worlds have a debug representation in the scene. Disable it by listing world names in the UnityModule constructor:
```c#
_systems = new ProtoSystems (new ProtoWorld (new Aspect1 ()));
_systems
    .AddModule (new UnityModule (new () {
        // Disable default world (null).
        // Disable world "Level" ("Level").
        DisableDebugWorlds = new[] { null, "Level" }
    }))
    .Add (new TestSystem1 ())
    .Init ();
```

### I want to add editing support for my component fields like for simple types. How?
Implement an inspector for a specific field type or the whole component:
```c#
// Component that needs an inspector.
public struct C1 {
    public string Name;
}

// Place this file somewhere in an Editor folder — it will be discovered automatically.
sealed class C1Inspector : ProtoComponentInspector<C1> {
    public override bool OnGuiTyped (string label, ref C1 value) {
        EditorGUILayout.LabelField ("Super C1 component", EditorStyles.boldLabel);
        string newValue = EditorGUILayout.TextField ("Name", value.Name);
        EditorGUILayout.HelpBox ($"Hello, {value.Name}", MessageType.Info);
        // Return false if unchanged.
        if (newValue == value.Name) { return false; }
        // Otherwise update and return true.
        value.Name = newValue;
        return true;
    }
}
```