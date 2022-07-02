# Contributing Guidelines

## Reporting a bug or requesting a feature

For now, the easiest way is to raise an issue in Github, either a bug or a
feature request. Other options will be created in the not too distant future.

## How to contribute changes

For the moment, TT isn't accepting any external contributions. This will change
once it reaches a stage where it is nearly ready to ship.

## Code Review Standards

Most seasoned engineers will know the difference between a well maintained and
a poorly maintained repository. In a well maintained repo, you can go in and
surgically make some minor changes to create great effects. In a poorly
maintained repo, you go in, struggle to figure out where the functionality is
that you're trying to change, try something, it breaks, try something else,
and some frustrating hours later it works, but you're not sure how or why.
To junior developers that probably sounds like an exaggeration.

The point of coding standards and code reviews is to keep the codebase clean,
simple, well structured, and easy to understand, so we can all be code surgeons
rather than scatter gunning changes hoping it'll work.

Below is a description of what I look for to keep this codebase clean by my
standards. This is an evolving document, updated whenever I find new issues
in proposed code, or change my mind about something. Always happy to consider
suggestions.

Due to the evolving nature of the document, it is quite possible that you'll
encounter existing code that doesn't meet these standards. If it's a small
change, feel free to include it in your PR. If it's bigger, raise a ticket.

If unsure - ask!

### Functionality

#### Does it do the thing

It should go without saying, but the number 1 critical thing is that it should
do what the ticket calls for. There should be a clear list of things that the
code implements that together close the bug / feature request. This list should
be in the pull request and the code should do that.

#### Easy to understand

The flow should be easy to understand, with functionality attached to an object
that makes sense. If I don't get it based on the comments, member names, and
code, then it's likely to be rejected.

#### Decoupled

Functionality should be decoupled in the sense that there shouldn't be lots of
MonoBehaviours calling each other. Sometimes it makes sense for a cluster of
objects to have some link, such as a list and its list items.

Ideally objects that are largely unrelated raise events to let the world know
about significant events and state changes, allowing other classes to subscribe
to them.

Within the TT solution, I don't feel the need for the overhead of implementing
interfaces in the main. There are few places where classes are swapped out or
common functionality handles multiple types of objects. Where this is the case,
there is common functionality so this uses a base class and inheritance
instead. E.g. see [WorldObjectBase.cs](https://github.com/ThatRobVK/Tabletop-Theatre/blob/3f5b0480a89b88434778288cf261988ea113923e/Assets/Scripts/World/WorldObjectBase.cs)

Interfaces are used where calling other code I have created. Third party
libraries often don't have interfaces, so direct dependencies are created. In
many cases these objects are used directly in Unity, which makes creating a
shim more difficult.

#### Common functionality

Where a piece of functionality is common across multiple use cases, it should
be created as a generic piece of code. For example the code in Helpers.cs, or
the TextBox class.

Ideally this functionality is extracted when code is used after the first time,
but where functionality seems generic enough it may be extracted on its first
use.

#### Additional changes

Minor refactoring in code files that are being changed as part of the work is
fine and unlikely to meet resistance. It is also fine for there to be larger
scale refactoring that is part of the change, e.g. moving code from TT to
TT.Shared where required. However larger scale refactoring that isn't part
of the change shouldn't be done in the same PR as this makes review harder
without any benefit to the functionality being implemented.

Where large refactoring opportunities are identified, create a new ticket
and work on it on a separate branch with a separate pull request.

### Tests

At the moment there are embarrassingly few tests in the game itself. This is a
work in progress. New functionality should be structured in a way that makes
it easy to test, and ideally have tests for any functionality that warrants it.
Pull requests won't be rejected for lack of tests. For now.

Tests are gladly accepted, however their structure will be carefully considered
to ensure they set the project up to easily expand the tests throughout the
codebase. Therefore don't be offended if changes are requested.

### File System

#### File location

Files go in locations that are as specific as their functionality. This means that
very generic UI code goes into `Scripts\UI`, while the `LoadButton.cs` file for
the main menu could sit as deep as `Scripts\UI\MainMenu\MapEditorMenu\`.

The most important thing is that these two factors are in sync. When a class is
first created it may go into a very specific location. If it is then re-used in
a broader context, it should be moved to a location that makes sense.

#### File name

The filename must match the class name, which in itself should be descriptive 
enough to guess its general purpose and what it is attached to.

### File Contents

#### Copyright statement
Must be present, standard format, copy/paste from another source file.

#### Using statements

No unused statements. Preferred order is System > Unity > Third party > TT, but
this isn't strictly enforced.

#### Namespace

Every file must have a namespace that matches its folder location, with the
root Scripts folder having the TT namespace. For example, a file in
`Assets\Scripts\UI\MapEditor\MainMenu` would have the namespace
`TT.UI.MapEditor.MainMenu`.

#### Naming standards

##### Methods, properties, fields

These should have descriptive names, typically describing what they do or
what the value they hold means.

##### Events and handlers

Events are named OnEventDescription, e.g. `OnButtonClick`. Handlers are named
HandleEventDescription, e.g. `HandleButtonClick`. Where there are multiple
handlers for the same event on different objects, include the object as
HandleObjectEventDescription, e.g. `HandleLoadButtonClick` and
`HandleSaveButtonClick`.

##### Casing

This follows the general C# naming standards as implemented by JetBrains Rider:

PascalCase:
- ClassNames
- PublicFields
- PublicProperties
- MethodNames

camelCase:
- editorFields
- _privateFields (note the leading underscore)
- localVariables
- methodParameters
- publicFieldsInSerializableClasses

UPPER_SNAKE_CASE:
- CONSTANT_VALUES

Note there is a difference in public fields between classes specifically
marked as Serializable (e.g. data classes that can be serialized for saving
and loading) and all other classes. When a class is explicitly marked as
Serializable, then public fields should use camelCase. All other classes,
including MonoBehaviours which are implicitly serializable, should use
PascalCase for public fields.

#### Editor fields

All editor fields should be private and have the [SerializeField] and
[Tooltip] attributes. The tooltip serves as the description, so no further
code comments are required on editor fields.

MonoBehaviours should have no public fields, these should be turned into
public properties to prevent them from being exposed in the Unity editor.

Using SerializeField instead of making the field public makes it explicit
that you intend for that field to be set through the editor, and prevents
accidental setting of fields that should only be accessed through code.

#### Static vs Instance

Typically all methods and properties are on the instance. However, when no
state is required, and the method or property is required by many other
classes which don't otherwise need a reference to an instance, then static
members are

#### Comments

All publicly exposed members (methods, properties, fields, events, etc.) and
all private methods must have XML documentation comments (&lt;summary&gt; etc).

The exception to this rule is Unity Lifecycle events such as `Start`, `Update`,
`OnDestroy` etc. These should not have any comments on the method itself as
all developers working on this codebase are expected to understand the basic
lifecycle. This should be compensated by slightly more verbose comments inside
the method on the actual code.

Event handler comments should take the form of "Called when <event happens>.
<Describe actions>", e.g. `Called when the load button is clicked. Loads the
selected map and switches to the Map Editor scene.`

Code should be descriptive and easy to understand based on method and variable
names. However, often it is useful to add a comment to describe a multi-part
if statement, or a block of code that creates a single outcome in multiple
lines of code.
  
Ideally classes themselves should have XML Documentation Comments as well,
describing what they are attached to if one or two specific objects / UI
elements, and what their general purpose is.

#### Code #regions

All but the most basic classes (e.g. with just one lifecycle event and one
private field) should be subdivided with `#region` and `#endregion`. The
following regions should be used, ideally in this specific order:

- Events
- Editor fields
- Private fields
- Public properties
- Lifecycle events
- Event handlers
- Public methods
- Private methods

However there should be no empty regions; only use the regions applicable
to the class.

#### Pragma warning disable

I used to use VSCode which didn't understand Unity or the version of C# used
by Unity. To get rid of the many warnings in the editor, many of the code
files have `#pragma warning disable IDExxxx` in them. I now use Rider which
has a much better Unity editing experience and doesn't require these lines.
I remove these lines as I come across them, but there may still be some.
