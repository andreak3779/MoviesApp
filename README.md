# Jubilant Waddle

This repository contains the **MovieCollectionApp**, a web application built with Angular.

## Project Structure

The project is organized as follows:

- **movie-collection-app/**: Contains the Angular application source code.
  - `src/`: Application source files.
  - `public/`: Static assets.
  - `.vscode/`: Visual Studio Code workspace configuration files.
  - `package.json`: Project dependencies and scripts.
  - `angular.json`: Angular CLI configuration.

## Getting Started

### Prerequisites

Ensure you have the following installed:

- [Node.js](https://nodejs.org/) (v16 or higher)
- [Angular CLI](https://angular.dev/tools/cli) (v21.1.1)

### Setup

1. Clone the repository:

   ```bash
   git clone https://github.com/your-repo/jubilant-waddle.git
   ```

2. Navigate to the project directory:

   ```bash
   cd jubilant-waddle/movie-collection-app
   ```

3. Install dependencies:

   ```bash
   npm install
   ```

### Running the Application

To start the development server, run:

```bash
npm start
```

Once the server is running, open your browser and navigate to [http://localhost:4200/](http://localhost:4200/). The application will automatically reload whenever you modify any of the source files.

### Building the Application

To build the project for production:

```bash
npm run build
```

The build artifacts will be stored in the `dist/` directory.

### Running Tests

#### Unit Tests

To execute unit tests using [Vitest](https://vitest.dev/), run:

```bash
npm test
```

#### End-to-End Tests

For end-to-end (e2e) testing, you can use a framework of your choice. Once set up, run:

```bash
ng e2e
```

### Additional Resources

For more information on using Angular CLI, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli).
