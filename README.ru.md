<p align="center">
    <img src="./logo.png" alt="Logo">
</p>

# Интеграция в редактор Unity для LeoECS Proto
Интеграция в редактор Unity с мониторингом состояния мира, рантайм для интеграции расширений для юнити.

> **ВАЖНО!** Требует C#9 (или Unity >=2021.2).

> **ВАЖНО!** Зависит от: [Leopotam.EcsProto](https://gitverse.ru/leopotam/ecsproto), [Leopotam.EcsProto.QoL](https://gitverse.ru/leopotam/ecsproto-qol).

> **ВАЖНО!** Не забывайте использовать `DEBUG`-версии билдов для разработки и `RELEASE`-версии билдов для релизов: все внутренние проверки/исключения будут работать только в `DEBUG`-версиях и удалены для увеличения производительности в `RELEASE`-версиях.

> **ВАЖНО!** Проверено на Unity 2021.3 (зависит от нее) и содержит asmdef-описания для компиляции в виде отдельных сборок и уменьшения времени рекомпиляции основного проекта.


# Социальные ресурсы
Официальный блог: https://leopotam.ru


# Установка


## В виде unity модуля
Поддерживается установка в виде unity-модуля через git-ссылку в PackageManager или прямое редактирование `Packages/manifest.json`:
```
"ru.leopotam.ecsproto.unity": "https://gitverse.ru/leopotam/ecsproto.unity.git",
```


## В виде исходников
Код так же может быть склонирован или получен в виде архива со страницы релизов.


## Прочие источники
Официальная работоспособная версия размещена по адресу [https://gitverse.ru/leopotam/ecsproto.unity](https://gitverse.ru/leopotam/ecsproto.unity), все остальные версии (включая *nuget*, *npm* и прочие репозитории) являются неофициальными клонами или сторонним кодом с неизвестным содержимым.


# Шаблоны кода в редакторе
Расширение добавляет в контекстное меню ресурсов проекта подменю `Create / LeoECS Proto`,
в котором можно выбрать автоматическую генерацию стартового кода без модулей, с модулями и сгенерировать пустой модуль 


# Подключение модуля
Модуль выполняет инъекцию именованных GameObject в системы, обеспечивает доступ к мирам для других unity-расширений,
выполняет интеграцию мониторинга состояния мира и систем в редактор:
```c#
// Инициализация окружения.
using Leopotam.EcsProto.Unity;

IProtoSystems _systems;

void Start () {        
    _systems = new ProtoSystems (new ProtoWorld (new Aspect1 ()));
    _systems
        // Подключение модуля интеграции.
        .AddModule (new UnityModule ())
        .Add (new TestSystem1 ())
        .Init ();
}
```

> **ВАЖНО!** Миры, которые нужно мониторить, должны быть подключены раньше модуля.

> **ВАЖНО!** По умолчанию названия компонентов записываются в имя `GameObject` на каждом изменении списка компонентов на
> сущности.
> Если такое поведение не нужно (например, для повышения производительности работы в редакторе), то его
> можно изменить через свойство `NoBakeComponentsInName = true` в параметре конструктора `UnityModule`.


После подключения модуля все миры будут доступны из любого места через методы `ProtoUnityWorlds`:
```c#
// Получаем подключенный мир по умолчанию.
ProtoWorld defaultWorld = ProtoUnityWorlds.Get ();
// Получаем подключенный мир с именем "events".
ProtoWorld eventsWorld = ProtoUnityWorlds.Get ("events");
```

Все объекты в сцене с компонентом `ProtoUnityLink` будут доступны из любого места через методы `ProtoUnityLinks`:
```c#
(GameObject go, bool ok) = ProtoUnityLinks.Get (linkName);
if (ok) {
    // Такой объект существует и с ним можно работать.
}
```

Для всех `ProtoUnityLink`-объектов поддерживается инъекция в поля систем через пометку специальным атрибутом:
```c#
class TestSystem : IProtoRunSystem {
    [DIUnity ("button1")] readonly GameObject _clickBtn = default;
    [DIUnity ("button1")] readonly Transform _clickBtnTransform = default;
    [DIUnity ("button1")] readonly Button _clickBtnButton = default;
    
    public void Run () {
        // Все поля системы должны быть проинициализированы к этому моменту
        // ссылками на один и тот же GameObject и компоненты на нем.
    }
}
```

> **ВАЖНО!** Отключение миров от глобального доступа происходит автоматически при вызове `IProtoSystems.Destroy()`.

> **ВАЖНО!** Сгенерировать стартовый класс можно через контекстное меню в редакторе unity.

# Визуальная сборка сущностей (авторинг)
Поддерживается сборка сущностей из компонентов путем сборки через инспектор из подготовленных компонентов.
Компонент считается подготовленным, если он помечен для сериализации и специальным атрибутом:

```c#
[Serializable, ProtoUnityAuthoring]
struct C1 {
    public Vector3 Position;
}
```

После это на `GameObject` в сцене добавляется компонент `ProtoUnityAuthoring`, через который производится сборка
через выбор компонентов из меню.

> **ВАЖНО!** Иерархия меню определяется автоматически на основе полных имен типов компонентов (пространство имен, имя
> типа).

Если требуется кастомная иерархия в меню, то ее можно явно указать через атрибут:

```c#
[Serializable, ProtoUnityAuthoring ("Игра/Бой/Юниты/Позиция")]
struct Position : IProtoUnityAuthoring {
    public Vector3 Position;
}
```

Иногда необходимо получить создаваемую сущность или GameObject, на котором висит авторинг - для этого следует
реализвать специальный интерфейс:

```c#
[Serializable, ProtoUnityAuthoring]
struct C1 : IProtoUnityAuthoring {
    public ProtoPackedEntityWithWorld Entity;
    
    // Получаем сущность и GameObject с авторингом.
    public void Authoring (in ProtoPackedEntityWithWorld entity, GameObject go) {
        Entity = entity;
    }
}
```


# Кастомные события с GameObject
Иногда требуется работать с ECS-событиями, исходные данных которых приходят исключительно в `MonoBehaviour` (события физики, uGui и т.д).
Так же иногда требуется передать `MonoBehaviour` как часть ECS-события.
Это можно сделать, реализовав наследника специального класса:
```c#
// ECS-событие, в котором будем получать наш класс-отправитель. 
struct MyTriggerEnterEvent {
    public MyTriggerEnterAction Sender;
}
class MyTriggerEnterAction : ProtoUnityAction<MyTriggerEnterEvent> {
    // Наши данные, специфичные для этого компонента.
    public int CustomId = 123;
    
    public void OnTriggerEnter(Collider c) {
        // Проверяем, можем ли отправлять событие.
        if (IsValidForEvent ()) {
            // Создаем событие.
            ref MyTriggerEnterEvent msg = ref NewEvent ();
            // Заполняем его поля.
            msg.Sender = this;
        }
    }
}
```
Компонент `MyTriggerEnterAction` вешается на `GameObject` и у него настраиваются нужные поля.
В момент срабатывания события входа в триггер этот компонент создаст ECS-событие, которое можно
будет поймать и обработать в ECS-системах.


# Поддержка Gizmos
Методы `Gizmos.XXX` обычно следует вызывать из `OnDrawGizmos()` внутри `MonoBehaviour`. В модуле интеграции есть штатный механизм, позволяющий вызывать эти методы из систем через реализацию специального интерфейса:
```c#
class TestGizmoSystem : IProtoRunSystem, IProtoUnityGizmoSystem {
    public void Run () {
        // Обработка данных.
    }
    public void DrawGizmos() {
        // Желательно оборачивать весь код для редактора
        // в специальный блок для вырезания его из билдов.
#if UNITY_EDITOR
        // Тут возможен вызов любого апи как в OnDrawGizmos().
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere (new Vector3 (1f, 2f, 3f), 1f);
#endif
    }
}
```


# Лицензия
Расширение выпускается под лицензией MIT-ZARYA, [подробности тут](./LICENSE.md).


# ЧаВо


### Я выполнил интеграцию, но не могу понять, где теперь смотреть состояние мира?
Нужно перейти в режим `PlayMode` и развернуть иерархию объектов в системной сцене `DontDestroyOnLoad` - там должны
появиться объекты с именами `[PROTO-WORLD]` для каждого мира.
Вся иерархия сущностей внутри этого объекта является виртуальной, создается только для визуализации наличия компонентов
на сущностях и не участвует в реальной работе ядра.


### У меня много сущностей и я хочу как-то отфильтровать только те, на которых есть нужные мне компоненты. Как это сделать?
По умолчанию имена типов компонентов добавляются к имени GameObject-а сущности, на которой они существуют - это можно
использовать для фильтрации штатными средствами
редактора Unity (строка поиска в окне иерархии сцены) для поиска GameObject-ов, подходящих под указанный шаблон.


### У меня несколько миров и я не хочу видеть интеграцию одного или нескольких из них в сцене. Как это сделать?
По умолчанию все подключенные миры имеют свое отладочное представление в сцене. Для его отключения можно перечислить имена миров в конструкторе `UnityModule`:
```c#
_systems = new ProtoSystems (new ProtoWorld (new Aspect1 ()));
_systems
    // Подключение модуля интеграции.
    .AddModule (new UnityModule (new () {
        // Отключено отображение мира по умолчанию (null).
        // Отключено отображение мира "Level" ("Level").
        DisableDebugWorlds = new[] { null, "Level" }
    }))
    .Add (new TestSystem1 ())
    .Init ();
```


### Я хочу добавить поддержку редактирования полей моих компонентов, как это сделано для простых типов. Как я могу это сделать?
Это можно сделать через реализацию инспектора для отдельного типа поля, либо компонента целиком:
```c#
// Компонент, к которому нужен инспектор.
public struct C1 {
    public string Name;
}

// Файл должен лежать где-то в проекте в папке Editor - будет обнаружен и подключен автоматически.
sealed class C1Inspector : ProtoComponentInspector<C1> {
    public override bool OnGuiTyped (string label, ref C1 value) {
        EditorGUILayout.LabelField ($"Super C1 component", EditorStyles.boldLabel);
        string newValue = EditorGUILayout.TextField ("Name", value.Name);
        EditorGUILayout.HelpBox ($"Hello, {value.Name}", MessageType.Info);
        // Если значение не поменялось - возвращаем false.
        if (newValue == value.Name) { return false; }
        // Иначе - обновляем значение и возвращаем true.
        value.Name = newValue;
        return true;
    }
}
```
