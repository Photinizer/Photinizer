
# Photinizer
[![Build](https://github.com/Photinizer/Photinizer/actions/workflows/ci.yml/badge.svg)](https://github.com/Photinizer/Photinizer/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/Photinizer/Photinizer?label=license)](https://github.com/Photinizer/Photinizer/blob/master/LICENSE.txt)

**Photinizer** is a fast, simple, and lightweight framework for building modern cross-platform desktop .NET applications without XAML, npm, or Electron.

It is designed for developers who know C# and basic web technologies and are looking for a straightforward "out-of-the-box" solution without the hassle:

- **Full-featured C# backend.**
- **Flexible UI** using HTML, CSS, and JS, with the ability to bring in the "heavy hitters" like Vue.js or React.
- **High-speed bidirectional RPC-style bridge** for seamless communication (featuring simple and convenient endpoint registration).
- **Project and item templates** to get you started instantly.

---
## Installation (Work in Progress)

Install the templates by running the following command in your terminal of choice:

```bash
dotnet new install Photinizer.Templates
```

---
## Getting Started

### Creating a Project (Work in Progress)

You can create a solution via Visual Studio/Rider or directly from the terminal:

```bash
// In the directory where you want your <SolutionName> folder to be created
dotnet new <TEMPLATE_NAME> -n <SolutionName>
```


| Template             | Shortcut     | Description                                                                                                                                 |
| -------------------- | ------------ | ------------------------------------------------------------------------------------------------------------------------------------------- |
| **Photinizer**       | `pzer`       | Includes the native Photinizer UI (Recommended). A very lightweight and easy-to-learn reactive JS framework that stays close to vanilla JS. |
| **Photinizer.Js**    | `pzer-js`    | Vanilla JS. No components, no reactivity. For those who want full control or a custom build.                                                |
| **Photinizer.Vue**   | `pzer-vue`   | Vue.js integration. A powerful UI platform, but requires npm and a more complex build pipeline.                                             |
| **Photinizer.React** | `pzer-react` | React.js integration. A powerful UI platform, but requires npm and a more complex build pipeline.                                           |

For more details, see *UI Options*.

### App Configuration (Early Stage — PhotinizerUI only)

The application is configured in two non-interchangeable places: `<YourProject>.App\appsettings.json` and `<YourProject>.App\Program.cs`

A standard `appsettings.json` looks like this (comments added for clarity):

```json
{
  "Photinizer": {
    "Title": "PhotinizerApp", // IMPORTANT: The value that will be displayed in the window title bar
    "Window": {
      "Width": 800,  // Sets the internal window width in pixels
      "Height": 900, // Sets the internal window height in pixels
      "Center": true, // Centers the window on the screen
      "DevToolsWhenDebug": true, // Since the UI runs in a browser, DevTools are available. Enabled in DEBUG by default.
      "DevToolsAlways": false    // If true, DevTools will be enabled in Release (ignoring DevToolsWhenDebug).
    },
    "UI": {
      "RootComponent": "Greetings" // VERY IMPORTANT: "Greetings" is a demo component. You should create your own and specify its class name here.
                                   // See the "PhotinizerUI" section for more details.
    }
  }
}
```

The beginning of `Program.cs` looks like this (comments added for clarity):

```CSharp
using Photinizer;
using Photinizer.UI.Own; // Required for the PhotinizerUI project (native UI)

new Photinizer()
    .AddOwnUI(Path.Combine("Frontend", "components")) // Adds native UI and specifies the path to components
    .Run(setup: o =>
    {
        // Example:
        o.Window.SetDevToolsEnabled(true); // You can further configure the PhotinoWindow here if needed
        
        o.Messenger // Register your endpoints here. See the "Messaging" section.
            .OnQuery("Hello, backend!", _ => "Hello, frontend!") 

```

---
## Messaging

To enable communication between the backend and the frontend, Photinizer provides a dedicated messaging mechanism consisting of both a frontend (JS) and a backend (C#) implementation.

Both parts are **symmetrical** and support the following message types:

- **Message**: A simple fire-and-forget message that doesn't require a response.
- **Task**: A request to start an operation. Supports awaiting completion (or handling errors).
- **Query**: A request to the opposite side that expects a return value (or an error).

> **Note:** It is assumed that the developer knows which endpoint is a Message, Task, or Query, and handles them accordingly on both sides.

### C# Side (Backend)

The backend implementation is handled by the `Messenger` class (found in `Photinizer/Messaging/Messenger.cs`).

```CSharp
// Sending (available in Program.cs by default) 
// NOTE: This concept is subject to significant improvements in future versions.
o.Messenger.SendMessage("some_frontend_endpoint1", new { arg1=arg1Value });
await o.Messenger.SendTask("some_frontend_endpoint2", new { arg1=arg1Value });
var result = await o.Messenger.SendQuery("some_frontend_endpoint3", new { arg1=arg1Value });

// Registering endpoints (supports method chaining)
o.Messenger
  .OnMessage("some_backend_endpoint1", el => DoSomethingAbout(el)) // Currently, 'el' is a JsonElement. Model parsing and dynamic support are being considered.
  .OnTask("some_backend_endpoint2", el => DoSomethingAbout(el)) // This task can be awaited on the frontend.
  .OnQuery("some_backend_endpoint3", el => GetSomeResultFor(el)) // Use .then() or await on the frontend to get the result.
  
  // Asynchronous versions are also available:
  .OnQueryAsync("some_backend_endpoint4", async el => { 
      await Task.Delay(1000); 
      return "SomeValue"; 
  });

```

### JS Side (Frontend)

The frontend implementation is called `PhotinizerMessenger` (found in `Photinizer.App/Frontend/wwwroot/photinizer-messenger.js`).  
It is exposed to your JS code via the `api` object.

```js
// Sending
api.message('some_backend_endpoint1', { arg1: arg1Value });
await api.task('some_backend_endpoint2', { arg1: arg1Value });
const result = await api.query('some_backend_endpoint3', { arg1: arg1Value });

// Callback alternative (if you prefer not to use async/await)
api.query('some_backend_endpoint4', { arg1: arg1Value })
  .then(result => doSomethingAbout(result))
  .catch(error => doAnotherThingAbout(error)); // Optional


// Registering endpoints
api.onMessage('some_frontend_endpoint1', data => doSomethingAbout(data));
api.onTask('some_frontend_endpoint2', data => doSomethingAbout(data)); // Can be awaited from the backend.
api.onQuery('some_frontend_endpoint3', data => getSomeResultFor(data)); // Must be awaited from the backend to receive the result.
```

---
## UI Options

### PhotinizerUI (Project: `Photinizer.UI.Own`) (In Development)

**PhotinizerUI** is a native reactive JS framework designed specifically for (and included with) Photinizer. It is extremely lightweight, fast, and easy to learn.

The biggest advantage: it requires **no package managers** (npm/yarn) and **no complex build steps**. It uses straightforward code that can be debugged directly in the browser/DevTools. Despite its simplicity, it is **component-based** (similar to React), where each component can manage its own styles, rendering logic, state, and properties.

**Simple Counter Example:**

```js
class Counter extends Component {
    constructor() {super({ count: 0, render: x => 
        /*html*/`
        <div>
            <p class="counter">${x.count}</p>
            <button onlick="${x.self()}.count++"></button>
        </div>`
    })}
}
```

>**Tip:** In VS Code, using the **es6-string-html** extension provides full syntax highlighting for HTML and CSS within your JavaScript files. We highly recommend installing it for a better development experience.

#### Component Syntax and Structure

Components in **PhotinizerUI** follow a specific structure:

>**Note on Dependency Resolution:** The `using` keyword is a custom Photinizer directive. The framework automatically parses these statements at runtime to resolve component dependencies, so you don't need to configure standard JS imports or bundlers.

```js
using someOtherComponent // Specify the filename without the .js extension
using folder/otherFolder/someComponentInFolder

class MyFirstComponent extends Component { // Inherit from Component (photinizer-ui.js)
    constructor() {
        super({ // Call the parent constructor and pass the configuration object
          field1: field1Value, // Changing these reactive fields triggers a re-render
          ...                  
          fieldN: field1Value, 
          
          private: {  // Optional: Section for non-reactive fields
              privateField1: privateField1Value, // Properties here are added to MyFirstComponent
              ...                                // but do not trigger re-renders
              privateFieldN: privateFieldNValue
          },

          // In 'stylize' and 'render', 'x' refers to the current instance of the component
          stylize: x => /*css*/ ` // Optional: Scoped styles added to the global style block on startup
          .selector1 { someStyles } 
          `,

          render: x => /*html*/` // Required: Returns the component's markup
          <anyTag> // Components must have a single root element
                // Insert component fields or JS expressions anywhere
                <anyTag>${x.field1}</anyTag>

                // Nested components are rendered simply:
                ${x.someOtherComponent.render()}
                ${x.label.render()}

                // Since handlers aren't executed immediately, you need a persistent 
                // reference to this instance. The self() function provides it.
                <anyTag onsomeevent="${x.self()}.someFunction()"/>

                // You can do the same for other component instances:
                <anyTag onsomeevent="${x.someOtherComponent.self()}.someFunction()"/>

                // find() returns a persistent reference to a component by its ID:
                <anyTag onsomeevent="${x.find(x.otherComponentId)}.someFunction()"/>
          </anyTag>
          `
      })
      
      this.someOtherComponent = new SomeOtherComponent();
      this.otherComponentId = this.someOtherComponent.id;
      
      // Property Binding:
      // If a property is bound, changing it does NOT re-render the entire component.
      // This is crucial for inputs to prevent them from losing focus.
      this.label = new Label().bindProp('text', this, 'field2', x => `Field2 is ${x.field2}!`);
      // Now, changing 'field2' only updates the Label instead of re-rendering the whole component.
    }
}

```


#### Component Management

- **Location:** Custom components are stored in `Photinizer.App` under `Frontend/components/users`.
- **Subfolders:** You can create nested folders, but ensure you include the path in the `using` statement.
- **Auto-loading:** Simply add your components to the folder, and Photinizer will automatically include them during the build process.

> **Note:** We recommend reviewing this structure carefully. You can find reference implementations in the `Greetings` component within `<YourApp>.App` and the `samples` folder in this repository.

**IMPORTANT:** The application window displays a single **Root Component**, which acts as a top-level container for all other nested components. By default, this is set to `Greetings`. You must specify your custom root component in `<YourApp>.App/appsettings.json`.

### Vanilla JS (Coming Soon)

### Vue.js (Coming Soon)

### React (Coming Soon)

---
## Powered by Photino.NET

This project is built on top of [**Photino.NET**](https://github.com/tryphotino/photino.NET), which utilizes the native system browser and Inter-Process Communication (IPC) for backend-frontend connectivity.

This approach ensures:

- **Minimal build size**: Unlike Electron, it doesn't bundle a browser with your app.
- **High-speed messaging**: Extremely fast communication between C# and JS.
- **No local server required**: Unless specifically required by your chosen frontend framework.

---
## Acknowledgments

- The **Photino.NET** project.
- Special thanks to [ivanvoyager](https://github.com/ivanvoyager) for his significant contributions and support.

---
## How to Contribute

Want to get involved? Join our community and reach out in our Telegram group: [t.me/Photinizer](t.me/Photinizer)

---
