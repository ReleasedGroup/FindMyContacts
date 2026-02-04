# Agentic Development Guide

# Agentic Development Platform – Developer User Guide

## Introduction

Welcome to the **Agentic Development Platform**, an AI-powered full-stack software development platform. This guide will walk you through using the platform from **Sprint 0** (inception) all the way to **production deployment**. We assume you are a software developer familiar with GitHub and common development tools. The platform combines the strengths of GitHub (for source control, planning and CI/CD) with AI coding assistants (OpenAI Codex CLI for backend, and Claude for frontend) to accelerate development. This approach yields numerous benefits – from accelerated development velocity and consistent code quality to reduced technical debt and faster time-to-market. By following this guide, you’ll be equipped to leverage the platform effectively from day one.

## Platform Overview and Setup

**GitHub as the Foundation:** The Agentic Development Platform is built on GitHub. All source code, documentation, plans, and even CI/CD workflows reside in your GitHub repositories. We use GitHub because it is an industry-standard platform that provides version control, collaborative workflows, integrated CI/CD, issue tracking, project management, and built-in code review and quality assurance processes. In other words, **100% of our workflow is repository-based**, ensuring everything is tracked and versioned centrally on GitHub. We even leverage **github codespaces** for cloud based development environment for our AI agents to run in.

**Initial Setup – Repository and Tools:**

1. **Create a GitHub Repository:** Begin by creating a new repository (e.g. github.com/ReleasedGroup/YourProject). Base it on our AgenticAITemplate. This template contains the core items needed to the whole system work. work closely with
   This will hold all project artifacts: requirements, specs, code, tests, and CI configuration.
2. **Create the new Repository** There are some limitations in the confgiuration of the codespace based off the template and you'll need to follow the procedure below to get everything working.
   1. Navigate to [github.com/ReleasedGroup](https://github.com/ReleasedGroup)
   2. Click **Repositories** from the top menu bar.
   3. Click **New Repository**.
   4. Give the Repository a **Name** and **Description**. We use the format ```client-project``` for the name and a description of that the project is meant to do.
   5. Ensure Visibility is set to **Internal**
   6. Start with a Template **ReleasedGroup/AgenticAITemplate**.
   7. Ensure Include All brances is set to Off.
   8. Click **Create repository**
3. Click **Code**, then in the **Codespaces** tab, click **Create codespace on main** to create a codespace for the repository. *This will set up a cloud-based development environment pre-configured with all necessary dependencies, including the AI agents.* The codespace will open and configure itself.
4. Run the following commands in the terminal to ensure you have the relevant tools available:
   ```bash
   chmod +x ./resolve-issue.sh
   chmod +x ./codex-interactive.sh
   chmod +x ./scripts/mcp-agentclient-linux-x64
   ```
5. Update **Devcontainer configuration**. Update the file /.devcontainer/devconiner.json as follows:
    ```json
      // .devcontainer/devcontainer.json
    {
    "name": "released-agentic-ai",
  
    "image": "mcr.microsoft.com/dotnet/sdk:10.0",
  
    "features": {
      // .NET SDK 10 (RC)
      "ghcr.io/devcontainers/features/dotnet:2": { "version": "10.0" },
  
      // Node.js (current LTS)
      "ghcr.io/devcontainers/features/node:1":   { "version": "lts" },
  
      // NEW → GitHub CLI (“gh”)
      "ghcr.io/devcontainers/features/github-cli:1": {}            // installs the latest gh release :contentReference[oaicite:0]{index=0}
    },
  
    "customizations": {
      "vscode": {
        "extensions": [
          "ms-dotnettools.csdevkit",
          "github.copilot",
          "dbaeumer.vscode-eslint",
          "openai.chatgpt",
          "anthropic.claude-code"
        ]
      }
    },
  
     "remoteEnv": { 
        "CODEX_HOME":"/workspaces/<Your Project>/codex"
    },
  
    "postCreateCommand": "npm i -g @openai/codex",
     // Start the agent on container start (headless, no attach required)
    "postStartCommand": "/workspaces/<Your Project>/scripts/mcp-agentclient-linux-x64 --server-url https://master-control-program.azurewebsites.net --project-key <Your Project Key>"

    }
    ```
    1. Update the  references to ```<Your Project>``` and ```<Your Project Key>``` accordingly.
6. **Update Codex Configuration**. The config.toml file which configures Codex CLI is located in the codex folder in the root of the repository. Here is an example:
    ```toml
    model = "gpt-5.1-codex-max"
    approval-policy="never"
    project_doc_max_bytes=256000
    web_search = true
    model_reasoning_effort = "medium"
    preferred_auth_method = "chatgpt"

    [sandbox]
    mode="danger-full-access"

    [shell_environment_policy]
    # inherit can be "core" (default), "all", or "none"
    inherit = "all"
    # set to true to *skip* the filter for `"*KEY*"` and `"*TOKEN*"`
    ignore_default_excludes = true


   [mcp_servers.microsoft_docs]
   command = "npx"
   args = ["-y", "mcp-remote", "https://learn.microsoft.com/api/mcp"]
   startup_timeout_sec = 120
   # Optional: override the default 60s per-tool timeout
   tool_timeout_sec = 30

   [mcp_servers.playwright]
   command = "npx"
   args= ["-y", "@playwright/mcp@latest"]
   startup_timeout_sec = 120
   # Optional: override the default 60s per-tool timeout
   tool_timeout_sec = 360

   [projects."/workspaces/<Your Project>"]
   trust_level = "trusted"

   [notice]
   hide_gpt5_1_migration_prompt = true
   hide_full_access_warning = true
   "hide_gpt-5.1-codex-max_migration_prompt" = true
    ```
    1. Update the reference to ```<Your Project>``` accordingly.

With the repository and tools ready, you can move into the project planning phase (Sprint 0).

## Sprint 0: Requirements and Technical Specification (Documentation First)

Sprint 0 is all about planning and **documentation-first design**. The goal is to capture **clear requirements and a technical blueprint** before writing any code. This ensures alignment with stakeholders and among the team before development begins. The Agentic Development Platform emphasises upfront documentation as a contract for what will be built.

### Writing the Requirements Specification

Start by drafting a **Requirements Specification** in Markdown within your repository. Create a file called ```docs/Requirements.md```. This document should include:

* **Business Objectives:** What is the project’s purpose and the high-level goals? Describe the problem being solved or the opportunity being addressed.
* **User Stories and Use Cases:** Capture the functionality from an end-user perspective. For each feature or requirement, write user stories (e.g. “As a <user type>, I want to <do something> so that <benefit>”) or plain use-case descriptions.
* **Functional Requirements:** List the specific capabilities the system must have. These can be derived from user stories but written as explicit “system shall do X” statements. Include acceptance criteria if possible.
* **Non-Functional Requirements:** List requirements like performance targets, security standards, usability, compliance, scalability, etc., that the system should meet.

Keep the language clear and unambiguous. Since this is a Markdown file, you can use Markdown formatting (headings, lists, etc.) to keep it structured. For example, use ## Use Cases and bullet points for each use case, etc. Storing this in GitHub means it’s version-controlled and you can track changes and feedback.

We use ChatGPT 5.1 Pro with Deep Research to create the requirements specification. I use a prompt like this:

```
Write a detailed requirements specification for a software project based
 on the following information: ... - [insert project description, user stories, 
 and any other relevant details here]
```

I may also add a meeting transcript or notes.

> It should be noted that ChatGPT in its current iterations allows you to download the results in a Word or PDF format. I use [markitdown](https://github.com/microsoft/markitdown) to convert Word documents to Markdown for easy inclusion in the repository. See the section below "From Word and PDF to Markdown (Documentation Workflow)" for more details.

> Customers commonly don't want to see markdown files. We have created a tool, md2word, that converts markdown files to Word documents for easy sharing with clients. See the section below on "From Markdown to Word to PDF (Documentation Workflow)" for more details.



### Drafting the Technical Specifications

Next, create a **Technical Specification & Sprint plan** document (e.g. ```docs/technical.md```). This will translate requirements into an actionable plan for architecture and development. Include the following:

* **Architecture Overview:** Describe the overall system architecture. This could include a high-level diagram of components (you can create a diagram and include it in the Markdown via an image, or provide a link). Identify the front-end client, backend services, database, and any integrations or external systems.
* **System Components:** Break down the system into components or services. For each component, describe its responsibility.
* **API Contracts and Endpoints:** List the API endpoints the backend will expose (if building an API). For each endpoint, specify the HTTP method, path, request parameters, and response format. This acts as a contract between frontend and backend.
* **Data Models and Database Schema:** Define the main data entities and their relationships. If using a relational database, you might include an ERD (entity-relationship diagram) or simply list tables and columns. If using NoSQL, describe collections and document structure.
* **Integration Points:** Describe any external APIs or services the system will integrate with (for example, payment gateways, third-party data sources) and how those will be accessed.
* **Sprint Plan:** Outline the plan for the first sprint (and possibly subsequent sprints at a high level). Identify which features or user stories will be tackled in Sprint 1, Sprint 2, etc. At minimum, define the scope of Sprint 1 in detail (since the platform promises going to production in Sprint 1). We will formally create sprint issues and milestones in the next section, but in the technical spec it’s good to list what is expected in the first increment.

Write the technical spec in Markdown as well. The **Documentation-First approach** means you get these requirements and design approved by stakeholders before coding. This ensures everyone agrees on scope and design, reducing changes later. The technical spec also serves as guidance for the AI agents – you can later refer to these details when prompting Codex or Claude to generate code (for example, copy relevant portions of the spec into the AI prompt so it knows the context).

### From Markdown to Word to PDF (Documentation Workflow)

Because stakeholders or non-technical clients might prefer polished documents, the platform encourages maintaining docs in Markdown (which is developer-friendly and versionable) and exporting to Word/PDF for readability.

The workflow is:

* **Author in Markdown:** Continue to refine Requirements.md and TechnicalSpecification.md in Markdown. Use simple formatting and keep it text-based. This keeps the content *LLM-safe* (easy for AI to parse if needed) and under version control in GitHub.
* **Convert to Word:** When you need to share with clients or management, convert the Markdown to a Word document. You can use a tool like our ```md2released``` for this. For example, to convert the requirements to Word, run a command:
* ```md2released --input <markdown> --output <docx> [--template <dotx>] [-c|--customer <name>] [-t|--title <title>]```
  * ```--input``` is the path to your Markdown file (e.g. docs/Requirements.md).
  * ```--output``` is the desired Word file path (e.g. docs/Requirements.docx).
  * ```--template``` is optional – you can provide a Word template (.dotx) with your company branding or styles. (We have a great ReleasedGroup template you can use.)
  * ```--customer``` and ```--title``` let you customise the document header/footer with client name and project title.
* This generates a Requirements.docx file. Open it in Microsoft Word to adjust any styling, add a cover page, table of contents or company branding as needed. The content should mirror the Markdown.
* **Embed Diagrams and Media:** In Word, you can insert more complex diagrams, charts, or images for clarity (e.g. an architecture diagram created in a drawing tool). These additions make the Word document “client-ready” with all necessary visuals. If you add images/diagrams, also save them in your repo (e.g. in a docs/images folder) so they are tracked.
* **Export to PDF:** After finalising the Word doc, export it as PDF for a final read-only version to distribute. This ensures no one accidentally edits the Word file and it’s easy to view.
* **Keep Markdown as Source of Truth:** Even after generating Word/PDF, always update the Markdown source for any changes. If stakeholders give feedback on the Word doc (for example, marking changes or comments), reflect those changes in the Markdown files. *Do not edit the Markdown by copy-pasting from Word blindly* – instead manually apply changes. **The key is to maintain the Markdown files as the authoritative version of documentation.**

By following this cycle (“**create in Markdown, release in Word/PDF, update in Markdown**”), you ensure documentation stays consistent, versioned, and easily maintainable by developers, while still presenting well to outside stakeholders.

At the end of Sprint 0, you should have: 
- Approved **requirements and technical spec** (in Markdown, with possibly Word/PDF exports). 
- A baseline GitHub repository with documentation and maybe skeleton code or at least a README.
- Development and AI tools configured on your codespace.
- A clear plan for Sprint 1 features.

Now you’re ready to plan and execute Sprint 1.

## Sprint Planning and Issue Management

With requirements and a spec in place, the next step is sprint planning. In the Agentic platform, we use **GitHub Issues and Milestones** to manage sprints and track progress.

### Creating a Sprint Backlog with GitHub Issues

**Sprint definition:** In GitHub, navigate to the **Issues** tab of your repository. If your project is using GitHub Projects or another planning tool, you can integrate that as well, but here we’ll stick to standard issues and milestones. For Sprint 1, create a milestone named “Sprint 1” (go to **Issues > Milestones** and create a new milestone, e.g. title "Sprint 1" with a due date if you have a timeline). Milestones represent sprints or time-boxed chunks of work.

> Here's the first time you can use agentic development to help you. Assuming you have the documents ready, the following procedure will get you milestones in and read in github:
> 1. Open your codespace terminal.
> 2. Run the command:
>   ```bash
>    codex
>   ```
> 4. Execute /approvals and choose Auto-approve all changes.
> 5. Once Codex CLI is running, provide the following prompt:
>    *Use gh to create all the sprints as milestones in github based on the sprint plan in docs/technical.md*


Now create **issues** for each task or user story planned in Sprint 1. Each issue should ideally trace back to a requirement or user stoSpry from your documentation. Good practices for issues: - Write a clear title (e.g. “Feature: User login page” or “Bug: Fix calculation error in pricing”). - In the issue description, provide detail or acceptance criteria. You can even paste in relevant excerpts from the spec (for example, the API contract or UI mock snippet for that feature) so that the developer or AI working on it has context. - Assign each issue to the “Sprint 1” milestone you created. Also, assign issues to yourself or team members as appropriate. - Label issues if needed (e.g. feature, bug, documentation, etc.) to categorise them.

Ensure that all the work for Sprint 1 is captured as issues in this way. This will be your sprint backlog. As work proceeds, you will track progress by closing issues and GitHub will show progress in the milestone (e.g. X of Y issues closed).

> Here's we can use agentic development again to help you. Assuming you have the documents ready, the following procedure will get you milestones in and read in github:
> 1. Open your codespace terminal.
> 2. Run the command:
>   ```bash
>    codex
>   ```
> 4. Execute /approvals and choose Auto-approve all changes.
> 5. Once Codex CLI is running, provide the following prompt:
>    *Use gh to create all the issues in github based on the sprint plan in docs/technical.md. Assign each issue to the appropriate milestone. Add a simple title and detailed description*

### Sprint Planning Meeting (if applicable)

If you work in a team, you might hold a sprint planning meeting to go over these issues, estimate them (you can use labels or comments to note estimates or priority), and confirm everyone’s understanding. Since this guide is developer-focused, we’ll assume you proceed to execution once the issues are defined.

### Developer Workflow for Issues

Each issue will go through a standard workflow in this platform. The recommended steps for working on an issue are:

**“Read Issue, Create Branch, Resolve Issue, Test, Document, Push, Create PR”** In practice, this means:

1. **Read the Issue:** Fully understand the task or bug. Make sure you have clarity on acceptance criteria. If something is unclear, add a comment to the issue for clarification before proceeding.
2. **Create a Branch:** In your local repository, create a new git branch dedicated to this issue. Use a naming convention that ties it to the issue, for example:

* git checkout -b feature/123-user-login
* (assuming issue #123 is “User login page”). This isolated branch will contain your work for the issue.

1. **Resolve the Issue (Code Implementation):** This is where development happens – using AI assistance where appropriate (detailed in the next sections for backend/frontend). Write the necessary code to implement the feature or fix the bug on this branch. Commit your changes in logical chunks with clear commit messages. For example, git add . then git commit -m "Implement login API endpoint and database model for User".
2. **Test the Solution:** Before considering the issue resolved, run local tests. If you have unit tests or integration tests, execute them (npm test or pytest or relevant command for your project). Ensure new features have corresponding tests written. We will cover testing in a dedicated section, but always verify that your change works as expected **and** does not break existing functionality (run the full test suite). If something fails, fix it and commit again.
3. **Document any Changes:** If the issue results in changes that affect documentation (for example, you changed an API contract or added a new configuration), update the Markdown docs accordingly on your branch. This keeps documentation in sync. If it’s a code-only change that doesn’t affect external behavior or specs, documentation might not need changes.
4. **Push Branch to GitHub:** When the feature is complete and tested locally, push the branch to the repository:

* git push -u origin feature/123-user-login
* This makes your branch visible on GitHub.

1. **Open a Pull Request (PR):** On GitHub, open a PR from your feature branch into the main (or development) branch. In the PR description, write "Closes #123" (to auto-link and close the issue on merge) and summarize the changes. The PR is where code review happens.
2. **Code Review and Merge:** The platform encourages both AI-assisted and human code reviews. GitHub will show a diff of your changes; team members (or you, if solo) should review the code. Optionally, an AI agent can also review the PR – for example, using a GitHub Action or bot that comments on the PR if it finds issues. Address any review comments by pushing additional commits. Once approved and all checks pass, merge the PR into main. This will automatically close the issue (if you used "Closes #Issue" syntax) and mark it complete.

Following this workflow ensures traceability: every code change ties back to an issue (and thus to a requirement). It also means your main branch always contains tested, reviewed code. Now, let’s delve into how to efficiently implement features using the AI tools for backend and frontend.

## Backend Development with OpenAI Codex CLI

For backend development tasks (such as creating API endpoints, database models, business logic, etc.), the Agentic platform leverages **OpenAI’s Codex CLI**. Codex CLI is an AI coding assistant you run in your terminal that can translate natural language prompts into code and even modify your project files.

*Figure: Example OpenAI Codex CLI interface running in a terminal. The CLI presents a splash screen and awaits natural language input for coding tasks.* The Codex CLI provides *“automated backend scaffolding from technical specifications”*[[12]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Automated%20backend%20scaffolding%20from%20technical,specifications). In practice, this means you can feed parts of your technical spec or user stories to Codex, and it will generate boilerplate code or even complete functions to meet those specs. Here’s how to use Codex CLI in your workflow:

### Using Codex CLI for a Backend Task

1. From the terminal, run the following command:
   ```bash
   ./resolve-issue.sh
   ```
2. Type the issue number
3. The system will resolve the issue, and create a fully tested and documented pull request. Once you review and approve the pull request, it will be merged automatically and the issue will be closed.

By using Codex CLI, you can rapidly scaffold backend features and ensure consistent patterns in your codebase (since the AI often follows common best practices by default). Always keep the human-in-the-loop: review and test everything the AI produces.

## Frontend Development with Claude

For frontend development (e.g. building UI components, implementing pages, managing state in a single-page app), the platform utilises **Claude** – an AI assistant known for conversational and coding capabilities, particularly effective in iterative refinement of tasks. Claude can help generate React components, suggest UX improvements, and integrate frontend with backend APIs.

**Using Claude for Frontend Excellence:** Claude’s strength lies in producing human-like, context-aware responses, which is great for front-end where understanding user experience is key. It can generate React code, HTML/CSS, and even suggest changes for accessibility or responsiveness. Here’s how to work with Claude in your frontend workflow:

1. **Setting up Context for Claude:** Just like with Codex, provide Claude with context. When starting a session with Claude (either in a chat interface or via an API client), begin by summarising your project and what you’re trying to build. For example: “I’m building a web app for X. We have a backend API that provides data Y. I need to build a React component for <specific feature>.” You might feed Claude the relevant part of the technical spec or even the API response format so it knows what data the frontend deals with.
2. **Request a Component or Feature:** Ask Claude for what you need. For example: *“Claude, please help me create a React component for a user login form with email and password fields, client-side validation, and calls the backend API at /api/login on submit.”* Claude will typically respond with an explanation and code. It may give you a full LoginForm component in JSX, including state handling (e.g. using React hooks) and perhaps some basic styling.
3. **Review and Incorporate the Code:** Take the code snippet from Claude and place it into your project (e.g. frontend/src/components/LoginForm.jsx). Check the code:
4. Does it follow your project’s coding style? (If not, you might need to adjust it or instruct Claude to follow a certain styleguide).
5. Does it correctly call your backend API as intended? (Make sure the endpoint URL and payload match your backend spec).
6. Are all necessary edge cases or validations handled? If not, you can prompt Claude further: “Add error handling for incorrect credentials” or “Improve this to show a loading spinner while the request is in progress.”
7. **Iterate with Claude:** Claude is conversational. If the first response isn’t perfect, you can say “That looks good, but can you refactor it to use Context API for managing auth state globally?” or “How about making the component mobile-responsive?”. Claude will then provide an updated version or suggestions. This iterative loop results in a more refined front-end component, much like pair-programming with a knowledgeable assistant.
8. **Integrate and Test in App:** Once you’re happy with the component, import it into the appropriate place in your application (e.g. into a page or parent component). Run your development server (npm start or equivalent) to see it in action in the browser. Test the interactions: for our login form example, enter sample data and see if it calls the backend and handles responses properly. Debug any issues – if you hit a tricky bug, you can even describe the bug to Claude and ask for help.
9. **Style and UX Refinements:** Claude can also assist with front-end styling or UX improvements. For instance, you can paste your CSS or JSX and ask, “Could you suggest improvements to make this more accessible?” It might point out missing alt attributes, low-contrast colors, or suggest using semantic HTML elements. You remain the decision-maker, but the AI can surface best practices.
10. **Manual Adjustments and Polish:** AI is helpful, but front-end often requires fine-tuning the details (pixel-perfect adjustments, specific branding etc.). Expect to do a bit of manual CSS tweaking or adding custom touches that AI might not nail. Use your judgement to polish the component after Claude’s contributions.
11. **Commit the Frontend Code:** Once the component/feature is working as intended, commit the changes on your issue branch. Write tests if applicable (e.g. component snapshot tests or integration tests using a tool like Jest + React Testing Library).

Claude helps ensure **“human-centered design with AI-powered implementation”, meaning you (the human developer) define the user experience and design choices, while the AI accelerates the implementation in modern frameworks like React. The result is polished UI code that aligns with best practices and is produced faster than hand-coding everything from scratch.

## Comprehensive Testing Strategy

Quality is a cornerstone of the Agentic Development Platform. AI assistance does not replace testing – in fact, the platform encourages an even more rigorous testing approach to catch any issues in AI-generated code. We employ a **comprehensive testing strategy** that covers multiple layers[[19]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Comprehensive%20Testing%20Strategy%20Testing%20Layers):

### Unit Testing

Each module or component should have unit tests validating its behaviour in isolation[[20]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Unit%20Testing%20Component,and%20method%20testing). For backend, this means writing tests for individual functions, classes or API endpoints (using tools like Jest, Mocha, pytest, JUnit, etc., depending on your language). For frontend, this could mean testing React components or utility functions (using Jest and React Testing Library or similar).

Practical steps: - Set up a testing framework early (if not already). Many project templates include one (e.g. Create React App sets up Jest, a Node/Express app might use Mocha/Chai, etc.). Ensure you can run tests with a command like npm test or yarn test. - Write tests for each feature you implement. You can even use AI to help generate tests. For example, after writing a function, ask Codex or Claude: *“Write unit tests for the above function using Jest.”* They might produce test cases, which you then verify and run. - Aim for good coverage on critical logic. If AI writes the code, be extra diligent in testing edge cases, as the AI might not foresee all of them. - Keep tests in a tests directory or alongside your code (e.g. files ending in .spec.js or .test.py). This ensures the CI pipeline can easily find and run them.

### Integration Testing

Beyond unit tests, write integration tests to ensure different parts of the system work together[[21]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Component,testing): - **Backend integration tests:** These could involve testing API endpoints against a test database. For example, using a tool like Supertest for Node to hit your Express app endpoints and verifying responses and database effects. Alternatively, using Python’s requests library against a locally running server in tests. - **Frontend integration tests:** Using frameworks like Cypress or Playwright to simulate user interactions in a browser environment. For instance, test that a user can go through a login flow hitting the real API (perhaps against a staging environment or a mocked API). - **End-to-End tests (E2E):** In some cases, you might set up a small staging environment and run E2E tests covering user journeys (this might be more relevant as the product stabilises, perhaps by Sprint 2 or 3, but it’s good to consider early if possible).

### Continuous Testing in CI

All tests (unit and integration) should be run automatically in the CI pipeline on each commit/pull request. We will configure this in the CI section next. The idea is to catch failures early.

In addition to tests, consider **code quality checks**: linters (for code style) and static analysis. For example, run ESLint for JavaScript/TypeScript or Pylint/Flake8 for Python as part of CI. These tools can catch syntax errors, undefined variables, or style issues that AI code might introduce. Include these in your pipeline to maintain consistent code quality.

By layering unit and integration tests, you ensure that AI-generated code and human-written code are both verified. The motto is trust but verify – even though our CI/CD will prevent untested code from deploying, the responsibility starts with writing good tests during development.

## Continuous Integration and Continuous Deployment (CI/CD) Pipeline

One of the platform’s key promises is going **“from commit to production in minutes, not days”**[**[22]**](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Automated%20Quality%20Gates%20at%20Every,production%20in%20minutes%2C%20not%20days). This is achieved via robust CI/CD pipelines on GitHub. We will use **GitHub Actions** for automation, though you could use any CI/CD tool (GitLab CI, Jenkins, etc.) – GitHub Actions is convenient since it’s integrated with our repo.

### Continuous Integration (CI)

Continuous Integration means every code change is automatically built and tested. We set this up as follows:

1. **Create a GitHub Actions Workflow:** If not done in Sprint 0, create a file in .github/workflows/ (for example, ci.yml). A basic Node.js example CI workflow might look like:

name: CI
on: [push, pull\_request]
jobs:
 build-and-test:
 runs-on: ubuntu-latest
 steps:
 - uses: actions/checkout@v3
 - uses: actions/setup-node@v3
 with:
 node-version: 18
 - run: npm install
 - run: npm run build --if-present
 - run: npm test

Adjust steps according to your tech stack (for Python, install dependencies and run pytest, for Java, run Maven/Gradle, etc.). The key is that on every push or PR, this workflow compiles the code (if needed) and runs all tests. 2. **Add Code Quality Checks:** Expand the CI workflow with additional steps like linting or type checking. For example, add a step - run: npm run lint if you have a lint script. This ensures code meets quality standards automatically. 3. **Configure Test Artifacts (if needed):** If tests produce coverage reports or screenshots (for front-end tests), configure Actions to upload those as artifacts or display results (some use coverage badges or comment on PR). This is optional but can be helpful. 4. **Parallelize or Matrix builds:** For larger projects, you might split jobs (e.g. separate frontend and backend tests). A matrix strategy can run tests on multiple configurations (like different Node versions or database types) if that’s important. This can be configured in the Actions YAML as well. 5. **Status Checks:** By default, GitHub will mark the commit or PR with a status (✅ pass or ❌ fail) based on the Actions result. In repository settings, you can enforce that PRs require a passing status check before merge. This is recommended so that no failing code can be merged.

When set up correctly, every time you push code (or open/update a PR), the CI pipeline will trigger. If a test fails or code doesn’t compile, you’ll know within minutes. This tight feedback loop helps maintain confidence in the codebase.

### Continuous Deployment (CD)

Continuous Deployment means that once code is merged to the main branch (and passes CI), it gets deployed to a production (or production-like) environment automatically[[23]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Continuous%20Deployment%20Automated%20deployments%20Environment,production%20in%20minutes%2C%20not%20days). The Agentic platform advocates to **“deploy early, deploy often”**, even from Sprint 1[[24]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Production%20Hosting%20from%20Sprint%201,ready%20infrastructure%20from%20day%20one). Here’s how to set up CD:

1. **Choose a Hosting Environment:** Determine where you will host the application. It could be a cloud provider like <CLOUD\_PROVIDER> (AWS, Azure, GCP) or a platform-as-a-service like Heroku, Vercel, Netlify (for frontends) etc. This guide will remain generic, but ensure by Sprint 1 you have at least a minimal environment available. For instance, an AWS account with an EC2 or an Azure Web App, etc., or a container registry if deploying via Docker.
2. **Provision Infrastructure:** Set up the necessary infrastructure for the app:
3. If using a cloud VM or container, create that resource (e.g. a Docker container host or Kubernetes cluster). If using serverless or PaaS, set up the app in that service.
4. Set up a database if needed (e.g. an RDS database or Cloud SQL instance) and any other backing services. Also, configure environment variables or secrets for the production environment (for example, API keys, database connection strings). Many cloud platforms let you store these securely.
5. Configure a domain name and **SSL certificate** if this is a web app. You want your production URL to be ready early (even if it’s just a placeholder site initially)[[25]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Infrastructure%20Setup%20Cloud%20hosting%20configuration,SSL%20certificates%20Monitoring%20and%20logging). For example, get a domain and provision HTTPS (you can use Let’s Encrypt or the cloud provider’s SSL services).
6. Set up **monitoring and logging**: integrate a service to collect logs (CloudWatch, Azure Monitor, etc.) and set up basic uptime monitoring. This ensures that when you deploy, you can track the app’s health[[25]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Infrastructure%20Setup%20Cloud%20hosting%20configuration,SSL%20certificates%20Monitoring%20and%20logging).
7. **Add Deployment Steps to Workflow:** Expand your GitHub Actions workflow or create a new one (e.g. cd.yml) to handle deployment on pushes to main branch. For example, you might add steps after tests to build a Docker image and push to a registry, then deploy:
8. For a containerised app: build the Docker image (docker build -t yourapp:${GITHUB\_SHA} .), push it to a registry (login to registry then docker push).
9. Then, trigger deployment: e.g. if using Kubernetes, apply the new image via kubectl; if using a PaaS, use their CLI or API to deploy; if using SSH to a VM, use scp/ssh to upload files. This will vary by environment.
10. Use GitHub Secrets to store credentials (like cloud API keys, or SSH keys). For example, in GitHub repo settings under **Secrets and variables**, add CLOUD\_API\_KEY or SSH\_PRIVATE\_KEY as needed, and use them in the workflow (e.g. environment variables in the YAML or using an action to set up cloud auth).
11. Example snippet for deploying to Azure Web App (just illustrative):

* - name: 'Deploy to Azure Web App'
   uses: azure/webapps-deploy@v2
   with:
   app-name: <AZURE\_APP\_NAME>
   slot-name: Production
   publish-profile: ${{ secrets.AZURE\_PUBLISH\_PROFILE }}
* Or for AWS:
* - name: 'Deploy to AWS EC2'
   uses: easingthemes/ssh-deploy@v2
   env:
   SSH\_PRIVATE\_KEY: ${{ secrets.EC2\_SSH\_KEY }}
   SSH\_HOST: ${{ secrets.EC2\_HOST }}
   with:
   commands: |
   docker pull <YOUR\_IMAGE>:${{ github.sha }}
   docker-compose up -d
* The exact code depends on your deployment target. The key is that after a successful CI run, this step brings the new code live.

1. **Verify Deployment:** After the action runs, it’s good to have some post-deployment checks:
2. You might run a simple smoke test script or ping an endpoint to verify the app is responding. This could be another step in the workflow or an external monitor.
3. If any step fails (deployment can fail if e.g. cloud creds are wrong), the Actions workflow will be marked failed. Set up notifications (GitHub can notify on workflow failures, or integrate with Slack/email) so the team knows to intervene.
4. The platform aims for automated quality gates – meaning code won’t deploy unless it passed tests, and won’t remain deployed if health checks fail[[23]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Continuous%20Deployment%20Automated%20deployments%20Environment,production%20in%20minutes%2C%20not%20days). So treat any failing deployment as a stop that must be fixed immediately.
5. **Rollback Strategy:** It’s wise to plan for rollbacks. For example, if a deployment goes out and a critical bug is discovered, you should be able to revert to the previous version quickly. Some strategies:
6. Keep the previous release’s image or artifact around so you can redeploy it.
7. Use deployment rings or slots (Azure has deployment slots, which allow quick swap; Kubernetes can keep previous ReplicaSets around, etc.).
8. Or simply use git: revert the commit on main that caused the issue and push, which triggers the workflow to deploy the last known good state.

By setting up CI/CD, you achieve continuous delivery of value. Starting this in Sprint 1 means your stakeholders can see a real, working product in a production environment immediately[[24]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Production%20Hosting%20from%20Sprint%201,ready%20infrastructure%20from%20day%20one). Early production deployment yields **real-world testing and feedback loops** from day one[[26]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Immediate%20Benefits%20Real,Stakeholder%20demos%20Early%20feedback%20loops). It also forces you to build with production quality from the start (security, config, and performance are considered early, not as afterthoughts).

## Production Deployment & Verification in Sprint 1

After the CI/CD pipeline runs for the first time on your merged Sprint 1 code, you will have a production instance running. Here’s how to handle the first deployment and beyond:

* **Access the Production Environment:** Go to the URL or IP of your deployed app. Ensure it’s up and serving requests. For a web app, open it in a browser and click around the features delivered in Sprint 1. This is essentially a demo for stakeholders. The platform philosophy is **“no waiting for production-ready – start production-ready”**[[27]](file://file_00000000305c7209a1fc66f278e4c137#:~:text=Cloud%20hosting%20configuration%20SSL%20certificates,Monitoring%20and%20logging), so treat this environment as if end-users might use it (even if initially it’s only testers or demo users).
* **Smoke Testing:** Do a quick run-through of core functionality in the prod environment. Sometimes differences in config (like database strings or API keys) can cause issues that didn’t appear in local testing. Verify those. Also check that static assets are loading, SSL is working (HTTPS lock icon in browser), etc.
* **Monitoring & Logging:** Check your monitoring dashboards to ensure the new deployment is reporting health. For example, see if any errors are being logged. Tools like Azure Application Insights or AWS CloudWatch Logs can be very helpful. Set up alerts for critical failures (e.g. if the website is down or an error rate spikes).
* **Stakeholder Demo:** Often at the end of Sprint 1 (or possibly each sprint), you’ll demo the working software to stakeholders. Since you have a live environment, you can share the URL with them to try out. This early feedback is invaluable. They may discover UI improvements or minor bugs – encourage them to log issues or provide feedback that you can convert into GitHub issues for Sprint 2.
* **Iterate for Next Sprint:** Use the feedback and any incomplete items to plan the next sprint. Update your documentation if requirements changed or new ideas emerged. Continue with the cycle: create issues for Sprint 2, use AI tools for implementation, write tests, and deploy.

## Conclusion and Best Practices

By following this user guide, you’ve set up a robust, AI-augmented development workflow: 
- You started with a strong planning foundation using **documentation-first** practices, ensuring clarity before coding. - 
- You leveraged **GitHub** for end-to-end project management – from issues and branches to pull requests and CI/CD integration, keeping everything transparent and version-controlled.
- You used **AI coding assistants** (OpenAI Codex CLI for backend, Claude for frontend) to accelerate development while maintaining quality and consistency. 

Remember that AI is a tool to boost productivity, but human oversight is key 

- always review AI outputs and steer the results. - 
- You implemented a rigorous **testing strategy** at multiple levels to catch issues early and give confidence in each release. - 
- You set up a fully automated **CI/CD pipeline** such that every commit that passes tests can be quickly deployed. This means your team can practice continuous delivery
  - releasing small increments frequently rather than big infrequent launches.
- You achieved **production deployment in Sprint 1**, unlocking immediate benefits like real user feedback, early discovery of any production issues, and a faster feedback loop for improvements.

As you continue using the Agentic Development Platform, keep in mind these best practices:
- **Keep Documentation and Code in Sync:** Whenever a change in direction or scope occurs, update the requirements or tech specs. This ensures the Markdown docs remain a living source of truth for the AI agents and new team members.
- **Small, Incremental Changes:** It’s better to have many small pull requests than a giant one. Small changes are easier to review (for both humans and AI) and debug when something goes wrong. This aligns with the sprint mindset of delivering value in slices. - 
- **Embrace AI-Human Collaboration:** Use AI to handle boilerplate and suggest solutions, but always apply your expertise for critical decision-making, creative design, and final quality control. The platform is about **“AI-human collaboration at its finest”** - neither the AI nor the human alone could achieve the same speed and quality.
- **Monitor Continuously:** Keep an eye on your CI pipeline results and production monitors. When something fails, treat it with urgency. A green pipeline and healthy production are the backbone of trust in this fast-paced approach.
- **Refine the Process:** In retrospectives, discuss how the AI tools are fitting into your flow. Perhaps you need to fine-tune prompts, or update the AGENTS.md with new guidelines if Codex or Claude made certain mistakes. Continuously improving how you use the platform will yield better outcomes over time.

With this guide, a developer should be able to jump in and use the Agentic Development Platform effectively from day one. By adhering to these steps and leveraging the full capabilities of GitHub and AI assistants, you will deliver production-ready software faster and more efficiently than traditional methods – truly *from requirements to production in a single sprint*. Good luck, and happy coding!


# Appendix One: From Word and PDF to Markdown

MarkItDown is a lightweight open-source Python tool (by Microsoft) for converting documents into Markdown text. It supports a wide range of formats – notably Word .docx and PDF – preserving the important structure of the document (headings, lists, tables, links, etc.) in Markdown form. This makes it ideal for developers who need to extract content for documentation, analysis, or feeding into Large Language Models (LLMs), rather than for high-fidelity formatting. Below, we’ll cover how to install MarkItDown on Windows, macOS, and Linux, configure its options, and use it to convert Word and PDF files to Markdown.

## System Requirements and Installation

**Prerequisites:** MarkItDown requires **Python 3.10+** to run. Ensure you have a compatible Python version installed on your system. It’s recommended to use a Python virtual environment to avoid dependency conflicts, but you can also install it system-wide. Internet access is needed to fetch the package and its dependencies from PyPI.

You can install MarkItDown via pip. The easiest method is to include *all* optional components (so it can handle every supported format) by using the [all] specifier. For example:

```python -m pip install "markitdown[all]"```

This single command will install MarkItDown and all its optional dependencies in your environment. After installation, verify it by checking the version:

```markitdown --version```

You should see a version number (e.g. markitdown 0.1.x) confirming the tool is installed. The instructions below detail platform-specific installation steps.

### Installing on Windows

1. **Install Python 3.10 or above** – If not already installed, download Python from the official website and run the installer. Make sure to enable the *“Add Python to PATH”* option during installation for convenience.
2. **Open a command prompt** (or PowerShell) and check the Python version: python --version. It should report 3.10 or higher.
3. *(Optional)* **Create a virtual environment** – This step is recommended for development. For example, run python -m venv venv and then activate it with venv\Scripts\activate. This ensures the MarkItDown installation won’t affect other projects.
4. **Install MarkItDown** – Use pip to install the package. On Windows, use double quotes around the package name with extras:
   ```python -m pip install "markitdown[all]"```
* This will fetch MarkItDown and all needed extras (including support for PDF and DOCX). *(If you only plan to convert specific formats, you could install a subset of extras. For example, pip install "markitdown[pdf,docx]" would install just the PDF and Word support.)*

1. **Verify the installation** – Run ```markitdown --version```. You should get an output indicating the version (e.g. markitdown 0.1.4). This confirms that the markitdown command-line tool is ready to use. If the command is not found, ensure your Python Scripts directory is in your PATH or use the full path to the markitdown.exe (in the Python Scripts folder).

> **Note:** On Windows, if the pip install command fails or the extras specifier isn’t recognized, double-check your quoting. Using "markitdown[all]" (double quotes) in Command Prompt/PowerShell will ensure the brackets are interpreted correctly. If you have multiple Python versions, you may need to use py -m pip install ... to target the right one.

### Installing on macOS

1. **Install Python 3.10+** – Recent macOS versions may not come with an up-to-date Python 3. You can install one via **Homebrew** (e.g. brew install python@3.11) or download the official Python installer for macOS. After installation, ensure the python3 command is available and is version 3.10 or higher (python3 --version).
2. **Open Terminal** and (optionally) create a virtual environment: for example, python3 -m venv ~/markitdown-env and source ~/markitdown-env/bin/activate.
3. **Install MarkItDown using pip**:
   ```python3 -m pip install 'markitdown[all]'```
   *(Using single quotes around markitdown[all] is fine in macOS/Linux shells to avoid shell expansion of the brackets.)* This will install MarkItDown along with all optional feature packages.
4. **Verify installation** by running: markitdown --version. It should display the version, confirming the tool is installed correctly.

If you encounter permission issues on macOS (e.g. using the system Python), consider using pip3 --user option or, better, use the virtual environment as suggested. Always make sure you’re installing into the intended Python environment (check with which python3 and which pip3 if needed).

### Installing on Linux

1. **Ensure Python 3.10+ is installed** – Most modern Linux distributions have Python 3 preinstalled. Check the version with python3 --version. If it’s older than 3.10, you may need to install a newer Python (for Debian/Ubuntu, you might use sudo apt-get update && sudo apt-get install python3.11 python3-pip or use **pyenv** for managing multiple Python versions).
2. **Open a terminal**. It’s recommended to create a virtual environment (e.g., python3 -m venv markitdown-env && source markitdown-env/bin/activate) to isolate the installation.
3. **Install MarkItDown via pip**:

* python3 -m pip install 'markitdown[all]'
* This installs MarkItDown with all its optional dependencies on your system[[5]](https://realpython.com/python-markitdown/#:~:text=%28venv%29%20%24%20python%20,all). If you prefer not to install unnecessary extras, you can install only what you need. For instance, to handle just Word and PDF, run pip install 'markitdown[pdf,docx]'[[8]](https://realpython.com/python-markitdown/#:~:text=Shell).

1. **Verify the installation** by running markitdown --version. You should see the version number output, confirming that the command is accessible.

On Linux, if markitdown isn’t found after installation, ensure that the Python user-base bin directory (often ~/.local/bin when using --user, or the virtualenv’s bin) is in your PATH. If using a virtual environment, remember to activate it before using the tool.

**Installing from source (optional):** If you want the latest development version or to contribute, you can clone the GitHub repo and install from source. For example: clone the repository and run pip install -e 'packages/markitdown[all]' in the project directory to install in editable mode[[9]](https://github.com/microsoft/markitdown#:~:text=Installation). This is not necessary for most users, as PyPI usually has the latest stable release.

**Docker alternative:** MarkItDown also provides a Docker setup if you prefer containerization. You can build the image with the provided Dockerfile and run conversions in an isolated container environment[[10]](https://github.com/microsoft/markitdown#:~:text=Docker). For instance: docker build -t markitdown:latest . then docker run --rm -i markitdown:latest < ~/input.pdf > output.md. This approach requires Docker but not a local Python setup.

## Configuration and Optional Settings

MarkItDown works out-of-the-box after installation, but there are a few configuration options and optional features to tailor its behavior:

* **Optional Dependencies:** MarkItDown’s functionality is modular. By installing with [all], you’ve included support for every format. If you choose a minimal install, ensure you have the needed extras for your use-case. For example, to convert Word .docx files you need the docx extra (which pulls in the **Mammoth** library for .docx parsing), and for PDFs you need the pdf extra (which uses **pdfminer** for PDF text extraction)[[11]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Office%20files%20are%20transformed%20into,BeautifulSoup)[[12]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=PDFs). If you get an error about a format not supported, it usually means the optional dependency is missing – reinstall MarkItDown with the appropriate extra or use [all][[13]](https://github.com/microsoft/markitdown/issues/1152#:~:text=read%20,For%20example).
* **Image Handling:** By default, MarkItDown will *not embed full image data* in the Markdown output. For example, if a Word document has embedded images, the resulting Markdown might show a truncated data URI or a placeholder rather than a huge base64 string. This is intentional to keep output lightweight. If you **want to include images** as base64-encoded data URIs in the Markdown, run the tool with the --keep-data-uris flag[[14]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L107-L111). For instance: markitdown --keep-data-uris file.docx -o file.md. With this option, any images encountered (in .docx, PDFs with images, etc.) will be embedded in full. Keep in mind that the Markdown file could become very large with embedded image data.
* **Output Customization:** The Markdown output preserves document structure but does not have many knobs to tweak the styling – it follows standard Markdown conventions. You cannot, for example, change how headings are numbered or how tables are formatted beyond the default. However, you can post-process the Markdown if needed. MarkItDown’s focus is on structure preservation (using # for headings, - or \* for lists, tables in Markdown format, etc.)[[2]](https://realpython.com/python-markitdown/#:~:text=1.%20Multi,in%20memory%20without%20creating%20temporary) rather than pixel-perfect layout.
* **Using Azure OCR (Document Intelligence):** If you need to extract text from **scanned PDFs or images** within PDFs (i.e. cases requiring OCR), MarkItDown can integrate with Microsoft’s Azure Document Intelligence service. You’ll need an Azure endpoint URL for the Document Intelligence API. Then you can run MarkItDown with -d (or --use-docintel) and -e <endpoint> options to offload conversion to the cloud OCR service[[15]](https://github.com/microsoft/markitdown#:~:text=To%20use%20Microsoft%20Document%20Intelligence,for%20conversion). For example:
* markitdown report\_scanned.pdf -o report.md -d -e "https://<your-doc-intel-endpoint>"
* This will send the file to Azure for processing and return Markdown output[[15]](https://github.com/microsoft/markitdown#:~:text=To%20use%20Microsoft%20Document%20Intelligence,for%20conversion). This feature requires an Azure account with the Document Intelligence (formerly Form Recognizer) resource, and you’ll incur costs for using that service. It can significantly improve text extraction for images/PDFs that don’t contain selectable text, and can preserve more structure (like layout, tables, even font styles) than the offline mode in some cases[[16]](https://realpython.com/python-markitdown/#:~:text=Sometimes%2C%20you%E2%80%99ll%20have%20an%20image,OCR). If you don’t provide the --endpoint when using -d, MarkItDown will prompt an error (the endpoint is required)[[17]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L80-L89)[[18]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L175-L183).
* **Plugins:** MarkItDown supports third-party plugins to extend its conversion capabilities. Plugins are off by default. You can list any installed plugins with markitdown --list-plugins and enable them by adding -p/--use-plugins to the command[[19]](https://github.com/microsoft/markitdown#:~:text=Plugins). Plugins can handle custom formats or post-processing. For instance, a plugin might add support for a format not natively covered. To find available plugins, you can search GitHub for the tag **#markitdown-plugin**[[20]](https://github.com/microsoft/markitdown#:~:text=To%20find%20available%20plugins%2C%20search,plugin). Unless you have installed specific MarkItDown plugins, you generally won’t need to use this option.
* **LLM Integration for Images:** By default, if MarkItDown encounters an image file or an image inside a document and you have not enabled data URIs or provided any vision AI, it may omit detailed content for that image. MarkItDown has an advanced feature where it can use a Large Language Model to generate a descriptive caption for images (e.g., describing the image content) instead of leaving them blank[[21]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Initially%2C%20image%20extraction%20might%20yield,no%20results). This requires configuring an LLM client in Python (not via the CLI directly). For example, you can use the Python API to initialize MarkItDown(llm\_client=..., llm\_model="gpt-4") to auto-generate image descriptions[[22]](https://github.com/microsoft/markitdown#:~:text=To%20use%20Large%20Language%20Models,llm_model). This is an optional, advanced feature for those integrating MarkItDown into AI workflows, and not needed just for basic file conversion.

In summary, out-of-the-box MarkItDown will convert text content and structure. The above options allow you to adjust what *extras* are included (images, OCR, plugins, etc.) and are there if you have specific needs.

## Usage: Converting Word and PDF Documents to Markdown

Once installed, using MarkItDown is straightforward. It provides a command-line interface (CLI) tool called markitdown that you run on your files, as well as a Python API for programmatic use. Here we focus on the CLI usage for converting Microsoft Word documents and PDFs into Markdown.

### Command-Line Examples

To convert a file, simply run markitdown <input-file>, and it will print the Markdown result to standard output (the terminal). You can redirect this output to a file or use the -o/--output option to specify a filename.

**Example 1: Convert a Word .docx file to Markdown**

Suppose you have example.docx (a Word document). To convert it:

markitdown example.docx -o example.md

This command will read **example.docx** and write the Markdown version to **example.md**[[23]](https://github.com/microsoft/markitdown#:~:text=Command). The output Markdown will contain the text and structure from the Word file. For instance, headings in the Word document become Markdown headings (# for Level 1, ## for Level 2, etc.), **bold text** in Word becomes \*\*bold\*\* in Markdown, *italic* becomes \*italic\*, and so on[[24][25]](https://realpython.com/python-markitdown/#:~:text=). Lists are converted to Markdown bullet points or numbered lists as appropriate. Hyperlinks in the document are preserved in Markdown format (e.g. [Link text](http://url)), and tables are rendered as Markdown tables.

Images embedded in the Word file will be referenced in the output. By default, as noted, the image data will be truncated (you might see something like ![image](data:image/png;base64,iVBORw0...==) with the data cut off). If you need the full image in the Markdown, use the --keep-data-uris flag as discussed earlier[[14]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L107-L111). Otherwise, you may choose to manually extract images or simply note their presence.

**Example 2: Convert a PDF file to Markdown**

Now take **report.pdf** (a PDF document). Conversion is just as easy:

markitdown report.pdf > report.md

Here we use shell redirection (>), which will save the output into **report.md**. You could alternatively use -o report.md as shown above. MarkItDown will extract text from the PDF and output it in Markdown format[[23]](https://github.com/microsoft/markitdown#:~:text=Command). **Note:** PDF conversion in MarkItDown is primarily text-focused – it will get the textual content out, but it may not preserve formatting like bold or headings in a meaningful way[[26]](https://realpython.com/python-markitdown/#:~:text=resource%2C%20reads%20it%2C%20and%20converts,it%20into%20Markdown)[[27]](https://realpython.com/python-markitdown/#:~:text=Markdown%20Syntax%20Demo). In fact, the Markdown output from PDFs often ends up as plain text paragraphs, since PDFs do not inherently preserve semantic structure (a heading in a PDF is just bigger/bold text; MarkItDown’s PDF parser might output it as plain text line). For example, if a PDF had a heading "Summary", the output might just include "Summary" as a line in the text without a # prefix. This is a known limitation – the PDF text is extracted accurately, but the markdown markup (like \*\* for bold, or lists) might be lost[[28]](https://realpython.com/python-markitdown/#:~:text=Python)[[29]](https://realpython.com/python-markitdown/#:~:text=Headings). The content is still there and readable, which is useful for text analysis or feeding into an LLM, but it’s not as neatly structured as a Word conversion would be.

If the PDF file **contains scanned images** (i.e. it's not text-based), the default conversion will likely produce little or no text. MarkItDown’s offline mode does not automatically run OCR on PDF images[[30]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files). In such cases, you would need to use the Azure Document Intelligence option (-d/-e) or perform OCR separately. For instance, using the -d mode as described earlier will leverage cloud OCR so that scanned PDFs can be converted[[30]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files). Alternatively, you might use an external OCR tool to get text from a scanned PDF, or consider using a different library specialized for OCR.

**Example 3: Using standard input and output pipes**

MarkItDown can read from standard input, which allows Unix-style piping. For example, you can do something like:

cat example.docx | markitdown > example.md

or

markitdown < report.pdf > report.md

Both of these feed the file content into MarkItDown and capture the output. Since MarkItDown normally detects the file type by extension, when using pipes you might need to hint the format. The CLI provides the -x/--extension option to specify the input type when reading from STDIN[[31]](https://github.com/1716775457damn/Markitdown/blob/6c9dd6a0cbde2171bfa80ab7a37ed7a7b5fe7cc6/markitdown_standalone.py#L96-L104)[[32]](https://github.com/1716775457damn/Markitdown/blob/6c9dd6a0cbde2171bfa80ab7a37ed7a7b5fe7cc6/markitdown_standalone.py#L136-L143). For example, markitdown -x .pdf < report.pdf > report.md ensures the tool knows the input is a PDF. In many cases, MarkItDown can auto-detect if the input is a PDF or text, but using the hint avoids any ambiguity.

**Example 4: Batch conversion of multiple files**

If you have many files to convert, you could script it. For instance, using a shell loop or a simple Python script. In Python, you could do something like:

from pathlib import Path
from markitdown import MarkItDown

md = MarkItDown()
for file\_path in Path("docs\_to\_convert").iterdir():
 if file\_path.suffix in (".docx", ".pdf"):
 result = md.convert(file\_path)
 output\_md = file\_path.with\_suffix(file\_path.suffix + ".md")
 output\_md.write\_text(result.markdown, encoding="utf-8")

The above Python snippet would iterate over files in a directory and convert each .docx or .pdf to Markdown (saving with an extra .md extension). This demonstrates the **API usage**: MarkItDown.convert() returns an object whose .markdown attribute holds the Markdown text[[24]](https://realpython.com/python-markitdown/#:~:text=)[[33]](https://realpython.com/python-markitdown/#:~:text=In%20this%20example%2C%20you%20call,from%20converting%20the%20input%20document). This approach is handy for automation, but for one-off conversions the CLI is usually easiest.

### What to Expect from the Output

After conversion, open the generated .md file in a text editor or Markdown viewer. For a Word document, you should see a neatly formatted Markdown document: top-level headings starting with #, bold and italic syntax where appropriate, bullet points or numbered lists matching the original, and tables formatted in Markdown table syntax. MarkItDown aims to preserve the *structure and content* rather than the exact visual layout[[34]](https://realpython.com/python-markitdown/#:~:text=HTML%20%2C%20and%20%2091,to%20create%20image%20captions%20and). This means things like font size or page breaks from Word are not retained, but the hierarchy of headings and list indentations are.

For a PDF, as mentioned, the output may be mostly plain text. You might need to manually add Markdown formatting if you intend to use the output for human-facing documentation. If high fidelity is required (e.g., preserving **bold**, *italics*, underlines, etc. from a PDF), a tool like **Pandoc** might serve better[[3]](https://realpython.com/python-markitdown/#:~:text=While%20MarkItDown%E2%80%99s%20output%20is%20often,readable%20conversions)[[35]](https://realpython.com/python-markitdown/#:~:text=If%20MarkItDown%20doesn%E2%80%99t%20meet%20your,needs%2C%20consider%20alternatives%20such%20as). Pandoc can often preserve formatting and even convert to formats beyond Markdown, but it’s slower and more complex to configure. MarkItDown trades some fidelity for speed and simplicity, optimising for use cases like quickly getting text out for AI pipelines[[36]](https://realpython.com/python-markitdown/#:~:text=To%20decide%20whether%20to%20use,conversion%20tasks%2C%20consider%20these%20factors).

In summary, using MarkItDown for Word and PDF is usually a one-command operation. Word .docx files convert to nicely structured Markdown with minimal effort. PDFs will yield the textual content, suitable for analysis or quick reference, but might need touch-ups if you want to use the Markdown for publishing.

## Supported File Types and Limitations

MarkItDown is not limited to Word and PDF; it supports a variety of file formats (which is one reason it’s so useful for aggregating content for analysis). The supported input types include[[2]](https://realpython.com/python-markitdown/#:~:text=1.%20Multi,in%20memory%20without%20creating%20temporary)[[37]](https://github.com/microsoft/markitdown#:~:text=MarkItDown%20currently%20supports%20the%20conversion,from):

* **Word documents** (.docx): Preserves headings, lists, basic formatting, etc. (Requires the docx extra – included if you installed [all].)
* **PowerPoint presentations** (.pptx): Each slide’s content is extracted (often as Markdown lists or headings). (Requires pptx extra.)
* **Excel spreadsheets** (.xlsx and legacy .xls): Tabular data is converted into Markdown tables[[38]](https://realpython.com/python-markitdown/#:~:text=in%20the%20downloadable%20materials%3A)[[39]](https://realpython.com/python-markitdown/#:~:text=,01). Each sheet is usually separated by a heading. (Requires xlsx/xls extras, which use pandas to read sheets.)
* **PDF files** (.pdf): Text content is extracted. Formatting (bold, italics, etc.) is mostly lost, and all text comes out as plain paragraphs[[28]](https://realpython.com/python-markitdown/#:~:text=Python)[[29]](https://realpython.com/python-markitdown/#:~:text=Headings). Does not handle scanned PDFs without OCR. (Requires pdf extra.)
* **Images** (.png, .jpg, etc.): If an image file is given, MarkItDown can embed metadata and a placeholder or caption. By default, it may not produce a useful caption unless an LLM is configured[[21]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Initially%2C%20image%20extraction%20might%20yield,no%20results)[[40]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Note%3A%20LLM%20won%27t%20deal%20with,OCR%20preprocessing%20to%20extract%20content). With an LLM client set, it can insert a descriptive caption for the image. Otherwise, you might only get the image filename or metadata. (Image OCR is only via Azure integration; there’s no offline OCR for images except basic EXIF text.)
* **Audio files** (.wav, .mp3): Audio is transcribed to text (English) using a speech recognition library. The output Markdown will be a transcript of the audio (with maybe simple formatting). (Requires the audio-transcription extra, which brings in something like the speech\_recognition package that by default calls Google’s API[[41]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Audio%20Files).)
* **HTML files or URLs** (.html or web URLs): HTML content is parsed and converted to Markdown. You can even give a URL (e.g., markitdown "http://example.com"), and MarkItDown will fetch the page and convert it[[42]](https://realpython.com/python-markitdown/#:~:text=)[[43]](https://realpython.com/python-markitdown/#:~:text=%3E%3E%3E%20result%20%3D%20md.convert%28,Example%20Domain). This is useful for quickly saving web content as Markdown.
* **Structured text formats**: JSON, CSV, XML files can be given to MarkItDown. CSV and Excel become tables in Markdown[[44]](https://realpython.com/python-markitdown/#:~:text=Once%20you%20have%20MarkItDown%20installed%2C,data%20about%20your%20company%E2%80%99s%20employees)[[45]](https://realpython.com/python-markitdown/#:~:text=%24%20cat%20employees.csv%20,11%2F5%2F2021), JSON and XML are formatted as code blocks or bullet points representing the data structure (exact output may depend on content).
* **ZIP archives** (.zip): MarkItDown will iterate through the archive’s contents and convert each file it knows how to handle, concatenating the results. This is handy to convert a whole archive of documents in one go[[46]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Image%3A%20Excel%20Example).
* **Emails** (.msg or .eml via Outlook extra): If you have the outlook extra, it can parse Outlook email files. The output includes headers (From, To, Subject) and the email body in Markdown.
* **YouTube videos** (via URL with youtube-transcription extra): Provide a YouTube video URL, and MarkItDown will attempt to fetch the transcript (if available) and output it as Markdown text[[47]](https://www.reddit.com/r/ObsidianMD/comments/1hioaov/microsoft_has_released_an_open_source_python_tool/#:~:text=Microsoft%20has%20released%20an%20open,Word)[[48]](https://www.reddit.com/r/ObsidianMD/comments/1hioaov/microsoft_has_released_an_open_source_python_tool/#:~:text=MarkItDown%20is%20a%20utility%20for,Word). This requires internet access and the youtube-transcription extra (which likely uses the YouTube API or an unofficial method to get subtitles).

Despite this broad format support, **there are known limitations** to be aware of:

* **Not a Visual Fidelity Converter:** MarkItDown’s goal is structured text for analysis. It is **not** focused on pixel-perfect or layout-perfect conversion. Complex layouts, fancy formatting, or exact styling will not carry over[[3]](https://realpython.com/python-markitdown/#:~:text=While%20MarkItDown%E2%80%99s%20output%20is%20often,readable%20conversions). For example, it won’t maintain font colors or exact spacing, and multi-column layouts will just be read in reading order.
* **PDF Formatting Loss:** As noted, when extracting from PDFs, MarkItDown often cannot distinguish headings or bold text reliably – the output is essentially plain text in Markdown wrapping[[30]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files)[[49]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=However%2C%20PDFs%20lose%20their%20formatting,plain%20text%20are%20not%20distinguished). Lists in a PDF might be turned into plain lines, and tables might become plain text or a list of lines. This is a limitation of using text extraction libraries on PDFs. If you require better preservation of structure from PDFs, you may need to use OCR or other tools.
* **Scanned Documents:** PDF files that are actually scans (images of text) **cannot be processed** by MarkItDown’s built-in PDF converter[[30]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files). You will get either an empty output or a very minimal output (perhaps just image references). To handle these, you must enable OCR (using the Azure option) or run an OCR tool beforehand. MarkItDown itself (offline) does not perform OCR on images.
* **Image Content:** Without an LLM integration, MarkItDown doesn’t generate alt-text or descriptions for images. It might include an image tag with a base64 blob (or truncated blob) if you use --keep-data-uris, but that’s about it. So, images are a weak spot – you either accept that you’ll handle them manually or employ the LLM plugin approach for automatic captions[[21]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Initially%2C%20image%20extraction%20might%20yield,no%20results)[[40]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Note%3A%20LLM%20won%27t%20deal%20with,OCR%20preprocessing%20to%20extract%20content).
* **Accuracy vs. Fidelity:** The content extracted is generally accurate text, but some **document formatting is lost**. Heavily styled Word documents (with complex nested lists or custom styles) might not convert perfectly. The structure is preserved “as much as possible”[[34]](https://realpython.com/python-markitdown/#:~:text=HTML%20%2C%20and%20%2091,to%20create%20image%20captions%20and), but you may find, for instance, that text boxes or sidebars in Word are just concatenated in the flow of text.
* **Large Files and Performance:** MarkItDown is reasonably fast, but extremely large files (hundreds of pages of PDF or very large Word docs) may take some time to process. It processes in-memory, which avoids temp files (good for speed)[[50]](https://realpython.com/python-markitdown/#:~:text=PowerPoint%20presentations.%203.%20In,create%20image%20captions%20and%20descriptions). Just ensure you have enough memory for big files. If you encounter performance issues, you might need to break documents into parts or consider whether all content is needed.
* **External Dependencies:** Because MarkItDown relies on several external Python libraries for parsing different formats, any bug or limitation in those libraries can affect output. For example, the Mammoth library (for .docx) might not perfectly handle some Word features like tracked changes or comments (those might be dropped). The pdfminer library might occasionally mis-order text if a PDF has unusual encoding. These edge cases are rare but possible.
* **Active Development:** MarkItDown is still a young project (as of 2025, version ~0.1.x). Breaking changes have occurred (for example, between 0.0.1 and 0.1.0 the interface for conversion changed)[[51]](https://github.com/microsoft/markitdown#:~:text=Breaking%20changes%20between%200,0)[[52]](https://github.com/microsoft/markitdown#:~:text=,not%20need%20to%20change%20anything). While you can rely on it for current use, be mindful that future updates might tweak how certain things are handled. Always check the release notes on the GitHub if you update the package.

If MarkItDown’s limitations are a blocker for your use-case, you might explore alternatives like **Pandoc** (which excels at high-fidelity document conversion) or other format-specific tools. Microsoft’s own Office suite or LibreOffice can convert documents to Markdown (with plugins) but are heavier solutions. For extracting text for AI pipelines, MarkItDown is usually the most convenient despite the above limitations, as it’s designed with that purpose in mind[[53]](https://realpython.com/python-markitdown/#:~:text=Be%20aware%20of%20these%20MarkItDown,could%20be%20the%20best%20choice).

## Troubleshooting Tips

Here are some common issues and solutions when using MarkItDown:

* **MarkItDown command not found:** If you get 'markitdown' is not recognized (Windows) or command not found (macOS/Linux), it means the tool isn’t in your PATH. Ensure you installed it in the correct Python environment. If using a virtual environment, activate it before running the command. On Windows, the Scripts folder of your Python or venv needs to be in PATH. On Unix, the bin directory of the venv or the ~/.local/bin (if installed for user) should be in PATH. You can always run it as python -m markitdown <file> as an alternative, which calls the module directly.
* **Optional dependency errors:** If you try to convert a file and MarkItDown throws an error about a missing converter or dependency, it likely means you didn’t install the needed optional package. For example, converting a PDF without the PDF extra might result in an error or no output. The fix is to reinstall with the proper extras. For instance, if you saw an error with PDF, do pip install markitdown[pdf] (or just reinstall with [all] to cover everything)[[13]](https://github.com/microsoft/markitdown/issues/1152#:~:text=read%20,For%20example). Similarly, for a Word .docx, ensure the docx extra is installed. In short, *“feature X not working”* → check that you have the [x] extra.
* **Installation installed an old version:** In some cases, using the latest Python (e.g. Python 3.14 at the time of writing) might cause pip to grab an older MarkItDown release (due to dependency wheels not being ready for the new Python version)[[54]](https://realpython.com/python-markitdown/#:~:text=Note%3A%20If%20you%E2%80%99re%20running%20the,earliest%20compatible%20version%20it%20finds). If markitdown --version shows a very low version (0.0.x), uninstall and try installing in a Python 3.13 or 3.12 environment, or check if a newer MarkItDown release is available for your Python version. This is a temporary issue when new Python versions come out and can be solved by using a supported Python or updating MarkItDown once it officially supports the new version.
* **Output Markdown has incomplete image data:** As noted, by default MarkItDown truncates image data URIs to avoid giant files. If you see an image tag with a base64 string that clearly is cut off (ending with ... or incomplete data), it’s intentional. Use --keep-data-uris if you need the full image embedded[[14]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L107-L111). Alternatively, extract the images manually (for example, if it’s a Word doc, you can unzip the .docx – since it’s a ZIP – and retrieve images from the word/media folder, then reference them in your Markdown).
* **Scanned PDFs output nothing useful:** If your PDF conversion output is empty or just a few odd characters, the PDF likely has no text layer (it’s all images). MarkItDown’s offline mode can’t read these[[30]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files). You will need to run OCR. Either use the -d Azure option if available[[30]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files), or use an external OCR tool (like Tesseract or Adobe) to get text, then you could format that to Markdown manually or semi-automatically.
* **Markdown output is too plain or lacks structure:** This is common with PDF input. If you expected more structured Markdown, remember that MarkItDown doesn’t magically reconstruct all formatting, especially for PDFs. For better structured output from PDFs, consider converting the PDF to Word first (using Adobe Acrobat or another converter) and then run MarkItDown on the Word file, or use Pandoc as an intermediary. In the case of Word documents, if the output is missing something (say a text box content didn’t show up), it might be a limitation of the Mammoth converter. You could try updating MarkItDown to the latest version (they continuously improve format support), or open the document and adjust it (e.g., ensure important content is in the main body, not in unsupported Word artifacts).
* **Encoding issues:** MarkItDown should handle Unicode well. But if you encounter odd characters or encoding-related errors, check that you used binary mode when redirecting streams. For instance, using type file.pdf | markitdown on Windows might not work correctly due to encoding issues; prefer providing the filename or using proper binary piping (cat on Linux/Mac is fine). If you see UnicodeDecodeError or similar in an error message, it might be reading binary as text by mistake – ensure you’re not forcing the input through a text pipe.
* **Using MarkItDown in code and getting deprecation warnings:** If you use the Python API, note that the returned object uses .markdown property for the output. Older examples used .text\_content which is now deprecated[[33]](https://realpython.com/python-markitdown/#:~:text=In%20this%20example%2C%20you%20call,from%20converting%20the%20input%20document). If you accidentally use that, you might get a warning. Switch to .markdown.
* **Crashes or unexpected errors:** If you hit a bug (it can happen given the project’s young age), check the GitHub issues page. MarkItDown is active on GitHub, and others may have reported the same issue. You might find workarounds or fixes in upcoming versions. Being open source, you can also contribute a fix or example to the project.

## Conclusion and Resources

MarkItDown makes it easy for developers to convert Word documents, PDFs, and many other file types into Markdown with minimal effort. By installing it with the necessary dependencies and using simple CLI commands, you can generate Markdown versions of documents for documentation, static site generators, or preprocessing for AI models. While it doesn’t preserve every visual detail (it prioritises text content and structure), it’s a fast and convenient tool for the intended use cases[[3]](https://realpython.com/python-markitdown/#:~:text=While%20MarkItDown%E2%80%99s%20output%20is%20often,readable%20conversions)[[36]](https://realpython.com/python-markitdown/#:~:text=To%20decide%20whether%20to%20use,conversion%20tasks%2C%20consider%20these%20factors).

For more information, you can refer to the official **MarkItDown GitHub repository**[[1]](https://github.com/microsoft/markitdown#:~:text=MarkItDown%20is%20a%20lightweight%20Python,document%20conversions%20for%20human%20consumption), which contains the README documentation, usage examples, and source code. The README on GitHub covers additional details and advanced integration (like the MarkItDown MCP server for LLM integration). Also, the project’s PyPI page[[55]](https://trigger.dev/docs/guides/python/python-doc-to-markdown#:~:text=Convert%20documents%20to%20markdown%20using,structured%20format%20for%20AI) provides a brief description. If you’re interested in deeper dives or community experiences, the Real Python tutorial on MarkItDown[[56]](https://realpython.com/python-markitdown/#:~:text=The%20MarkItDown%20library%20lets%20you,powered%20workflows) and the developer blog posts (e.g., on Medium and Dev.to) are great resources for learning tips and seeing MarkItDown in action.

With this guide, you should be equipped to install, configure, and use MarkItDown to convert Word and PDF files to Markdown format. Happy converting!

**Sources:**

* Microsoft MarkItDown GitHub – *“Python tool for converting files and office documents to Markdown.”* [[1]](https://github.com/microsoft/markitdown#:~:text=MarkItDown%20is%20a%20lightweight%20Python,document%20conversions%20for%20human%20consumption)[[37]](https://github.com/microsoft/markitdown#:~:text=MarkItDown%20currently%20supports%20the%20conversion,from)[[57]](https://github.com/microsoft/markitdown#:~:text=Prerequisites)[[9]](https://github.com/microsoft/markitdown#:~:text=Installation)[[23]](https://github.com/microsoft/markitdown#:~:text=Command)
* *Real Python:* “Python MarkItDown: Convert Documents Into LLM-Ready Markdown” – Installation and usage details[[5]](https://realpython.com/python-markitdown/#:~:text=%28venv%29%20%24%20python%20,all)[[8]](https://realpython.com/python-markitdown/#:~:text=Shell)[[2]](https://realpython.com/python-markitdown/#:~:text=1.%20Multi,in%20memory%20without%20creating%20temporary)[[24]](https://realpython.com/python-markitdown/#:~:text=)[[28]](https://realpython.com/python-markitdown/#:~:text=Python)
* *Dev.to:* “Deep Dive into Microsoft MarkItDown” – Insights on limitations (PDF OCR, formatting)[[58]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files)[[49]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=However%2C%20PDFs%20lose%20their%20formatting,plain%20text%20are%20not%20distinguished)
* *GitHub Discussions:* MarkItDown Q&A – Example of image conversion issue and solution (use of --keep-data-uris)[[14]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L107-L111)
* *GitHub Issue:* Optional dependency installation – notes on including [pdf] or [docx] extras to resolve missing converter errors[[13]](https://github.com/microsoft/markitdown/issues/1152#:~:text=read%20,For%20example)
* *MarkItDown README:* Features, plugins, and Azure integration usage[[19]](https://github.com/microsoft/markitdown#:~:text=Plugins)[[15]](https://github.com/microsoft/markitdown#:~:text=To%20use%20Microsoft%20Document%20Intelligence,for%20conversion)
* *Real Python:* Limitations of MarkItDown vs Pandoc[[3]](https://realpython.com/python-markitdown/#:~:text=While%20MarkItDown%E2%80%99s%20output%20is%20often,readable%20conversions)[[35]](https://realpython.com/python-markitdown/#:~:text=If%20MarkItDown%20doesn%E2%80%99t%20meet%20your,needs%2C%20consider%20alternatives%20such%20as).

[[1]](https://github.com/microsoft/markitdown#:~:text=MarkItDown%20is%20a%20lightweight%20Python,document%20conversions%20for%20human%20consumption) [[4]](https://github.com/microsoft/markitdown#:~:text=MarkItDown%20requires%20Python%203,environment%20to%20avoid%20dependency%20conflicts) [[9]](https://github.com/microsoft/markitdown#:~:text=Installation) [[10]](https://github.com/microsoft/markitdown#:~:text=Docker) [[15]](https://github.com/microsoft/markitdown#:~:text=To%20use%20Microsoft%20Document%20Intelligence,for%20conversion) [[19]](https://github.com/microsoft/markitdown#:~:text=Plugins) [[20]](https://github.com/microsoft/markitdown#:~:text=To%20find%20available%20plugins%2C%20search,plugin) [[22]](https://github.com/microsoft/markitdown#:~:text=To%20use%20Large%20Language%20Models,llm_model) [[23]](https://github.com/microsoft/markitdown#:~:text=Command) [[37]](https://github.com/microsoft/markitdown#:~:text=MarkItDown%20currently%20supports%20the%20conversion,from) [[51]](https://github.com/microsoft/markitdown#:~:text=Breaking%20changes%20between%200,0) [[52]](https://github.com/microsoft/markitdown#:~:text=,not%20need%20to%20change%20anything) [[57]](https://github.com/microsoft/markitdown#:~:text=Prerequisites) GitHub - microsoft/markitdown: Python tool for converting files and office documents to Markdown.

<https://github.com/microsoft/markitdown>

[[2]](https://realpython.com/python-markitdown/#:~:text=1.%20Multi,in%20memory%20without%20creating%20temporary) [[3]](https://realpython.com/python-markitdown/#:~:text=While%20MarkItDown%E2%80%99s%20output%20is%20often,readable%20conversions) [[5]](https://realpython.com/python-markitdown/#:~:text=%28venv%29%20%24%20python%20,all) [[6]](https://realpython.com/python-markitdown/#:~:text=This%20command%20installs%20MarkItDown%20and,the%20package%20is%20working%20correctly) [[7]](https://realpython.com/python-markitdown/#:~:text=Shell) [[8]](https://realpython.com/python-markitdown/#:~:text=Shell) [[16]](https://realpython.com/python-markitdown/#:~:text=Sometimes%2C%20you%E2%80%99ll%20have%20an%20image,OCR) [[24]](https://realpython.com/python-markitdown/#:~:text=) [[25]](https://realpython.com/python-markitdown/#:~:text=) [[26]](https://realpython.com/python-markitdown/#:~:text=resource%2C%20reads%20it%2C%20and%20converts,it%20into%20Markdown) [[27]](https://realpython.com/python-markitdown/#:~:text=Markdown%20Syntax%20Demo) [[28]](https://realpython.com/python-markitdown/#:~:text=Python) [[29]](https://realpython.com/python-markitdown/#:~:text=Headings) [[33]](https://realpython.com/python-markitdown/#:~:text=In%20this%20example%2C%20you%20call,from%20converting%20the%20input%20document) [[34]](https://realpython.com/python-markitdown/#:~:text=HTML%20%2C%20and%20%2091,to%20create%20image%20captions%20and) [[35]](https://realpython.com/python-markitdown/#:~:text=If%20MarkItDown%20doesn%E2%80%99t%20meet%20your,needs%2C%20consider%20alternatives%20such%20as) [[36]](https://realpython.com/python-markitdown/#:~:text=To%20decide%20whether%20to%20use,conversion%20tasks%2C%20consider%20these%20factors) [[38]](https://realpython.com/python-markitdown/#:~:text=in%20the%20downloadable%20materials%3A) [[39]](https://realpython.com/python-markitdown/#:~:text=,01) [[42]](https://realpython.com/python-markitdown/#:~:text=) [[43]](https://realpython.com/python-markitdown/#:~:text=%3E%3E%3E%20result%20%3D%20md.convert%28,Example%20Domain) [[44]](https://realpython.com/python-markitdown/#:~:text=Once%20you%20have%20MarkItDown%20installed%2C,data%20about%20your%20company%E2%80%99s%20employees) [[45]](https://realpython.com/python-markitdown/#:~:text=%24%20cat%20employees.csv%20,11%2F5%2F2021) [[50]](https://realpython.com/python-markitdown/#:~:text=PowerPoint%20presentations.%203.%20In,create%20image%20captions%20and%20descriptions) [[53]](https://realpython.com/python-markitdown/#:~:text=Be%20aware%20of%20these%20MarkItDown,could%20be%20the%20best%20choice) [[54]](https://realpython.com/python-markitdown/#:~:text=Note%3A%20If%20you%E2%80%99re%20running%20the,earliest%20compatible%20version%20it%20finds) [[56]](https://realpython.com/python-markitdown/#:~:text=The%20MarkItDown%20library%20lets%20you,powered%20workflows) Python MarkItDown: Convert Documents Into LLM-Ready Markdown – Real Python

<https://realpython.com/python-markitdown/>

[[11]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Office%20files%20are%20transformed%20into,BeautifulSoup) [[12]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=PDFs) [[21]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Initially%2C%20image%20extraction%20might%20yield,no%20results) [[30]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files) [[40]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Note%3A%20LLM%20won%27t%20deal%20with,OCR%20preprocessing%20to%20extract%20content) [[41]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Audio%20Files) [[46]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=Image%3A%20Excel%20Example) [[49]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=However%2C%20PDFs%20lose%20their%20formatting,plain%20text%20are%20not%20distinguished) [[58]](https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5#:~:text=,when%20extracting%20from%20PDF%20files) Deep Dive into Microsoft MarkItDown - DEV Community

<https://dev.to/leapcell/deep-dive-into-microsoft-markitdown-4if5>

[[13]](https://github.com/microsoft/markitdown/issues/1152#:~:text=read%20,For%20example) markitdown optional dependency installation · Issue #1152 · microsoft/markitdown · GitHub

<https://github.com/microsoft/markitdown/issues/1152>

[[14]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L107-L111) [[17]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L80-L89) [[18]](https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py#L175-L183) \_\_main\_\_.py

<https://github.com/microsoft/markitdown/blob/dde250a456d178fe344fce17ef10d00fe929f680/packages/markitdown/src/markitdown/__main__.py>

[[31]](https://github.com/1716775457damn/Markitdown/blob/6c9dd6a0cbde2171bfa80ab7a37ed7a7b5fe7cc6/markitdown_standalone.py#L96-L104) [[32]](https://github.com/1716775457damn/Markitdown/blob/6c9dd6a0cbde2171bfa80ab7a37ed7a7b5fe7cc6/markitdown_standalone.py#L136-L143) markitdown\_standalone.py

<https://github.com/1716775457damn/Markitdown/blob/6c9dd6a0cbde2171bfa80ab7a37ed7a7b5fe7cc6/markitdown_standalone.py>

[[47]](https://www.reddit.com/r/ObsidianMD/comments/1hioaov/microsoft_has_released_an_open_source_python_tool/#:~:text=Microsoft%20has%20released%20an%20open,Word) [[48]](https://www.reddit.com/r/ObsidianMD/comments/1hioaov/microsoft_has_released_an_open_source_python_tool/#:~:text=MarkItDown%20is%20a%20utility%20for,Word) Microsoft has released an open source Python tool for converting ...

<https://www.reddit.com/r/ObsidianMD/comments/1hioaov/microsoft_has_released_an_open_source_python_tool/>

[[55]](https://trigger.dev/docs/guides/python/python-doc-to-markdown#:~:text=Convert%20documents%20to%20markdown%20using,structured%20format%20for%20AI) Convert documents to markdown using Python and MarkItDown

<https://trigger.dev/docs/guides/python/python-doc-to-markdown>


# Appendix Two: From Markdown to Word to PDF (Documentation Workflow)

# Appendix Three: Master Control Program

The Master Control Program (MCP) is a web-based system for orchestrating AI coding agents to automate software development tasks. It is built with Blazor WebAssembly (WASM) hosted by an ASP.NET Core 9 backend and runs on Azure. The MCP manages multiple software projects (each corresponding to a GitHub repository), tracks development tasks, and controls “Agentic AI Developer” instances that implement those tasks. The primary goal is to streamline development by delegating coding tasks to AI agents (powered by OpenAI Codex) while providing a clear interface for human oversight.

## System Overview

The MCP consists of a Blazor front-end and a back-end service working together to manage projects, tasks, and AI agent workflows. Key components include:

- **Project Tracker**: Maintains a list of projects (GitHub repos) with their details, outstanding issues, and current sprint (milestone) information.
- **Task Queue**: Stores tasks to be performed by AI agents, including priorities, dependencies, and deadlines.
- **AI Agent Orchestrator**: Launches and monitors Agentic AI Developer sessions in GitHub Codespaces for executing tasks, including creating Codespaces for specific task types (e.g., Run Tests, Apply Patch), logging progress steps ("Creating Codespace…", "Codespace ready.") in the Task Details view.
- **User Interface**: A Blazor web UI styled with Tailwind CSS for a modern, responsive design. The global navigation includes Sign in with GitHub and Logout actions and shows a "Signed in with GitHub" indicator when a session is active.
- **Integration Services**: Connections to external services including user authentication with GitHub via OAuth and the GitHub API for repository data (issues, pull requests, milestones), and Azure AD (Microsoft Entra ID) for application authentication.

## Technical Highlights

The MCP is a Blazor-based ASP.NET Core 9 web application leveraging GitHub Codespaces and OpenAI Codex for AI-driven code generation and analysis. It uses SignalR for real-time progress updates, Tailwind CSS for responsive UI styling, and integrates with GitHub APIs and Azure AD (Microsoft Entra ID) for authentication and repository management. It is also a Progressive Web App (PWA) supporting full-screen install and offline capability on desktop, tablet, and mobile. The application runs on .NET 9 and is hosted on Azure App Service.

## Authentication and Access

- Pages are hidden until you sign in with GitHub. When not signed in, only the Home page (`/`) is accessible and the navigation menu does not show links to dashboards or settings.
- After signing in via GitHub OAuth, the navigation reveals Project Dashboard, Task Dashboard, and Settings. Individual pages defensively verify authentication and redirect to `/` when no session is present.
- The client centralizes session checks via a shared `IAuthService` (`api/ping` probe) injected into pages, eliminating duplicated `IsSignedInAsync` methods across components.
- Agent endpoints continue to support `X-Project-Key` for headless access as documented in src/AGENTS.md; UI access is strictly gated by GitHub sign-in.

## Setup and Configuration

### Codex CLI Setup (required for agents)

The Agent Client invokes the OpenAI Codex CLI to execute prompts and generate code.

- Install the Codex CLI and ensure it is available on your PATH.
  - Verify: `codex --version` and `codex help` should succeed.
- Set your OpenAI API key so Codex can authenticate:
  - Bash: `export OPENAI_API_KEY=sk-...`
  - PowerShell: `$env:OPENAI_API_KEY = "sk-..."`
- Optional: configure sandbox and shell policies in `codex/config.toml` (in this repo) or your user config directory.
  - Example settings used by the agent:
    - `model = "gpt-5.1-codex-max"`
    - `approval-policy = "never"`
    - `[sandbox] mode = "danger-full-access"`
- Before invoking Codex, the agent checks out `main` and pulls the latest changes (`git checkout main && git pull`) to ensure it works from the up-to-date default branch.
- The agent executes Codex as: `codex exec -s danger-full-access "<PROMPT>"`.
  - When a GitHub issue number is available, the Standard Prompt is: `Use gh to read #<ISSUE_NUMBER>. Resolve the issue. Create a new branch, add commit, push and create a detailed PR. Add the text 'Closes #<ISSUE_NUMBER>' to the PR description`.
  - Prompt templates are configurable:
    - Global Standard Prompt (Admin > Settings) applies to all projects/tasks when no project or task template is set.
    - Project-level Prompt Template can override the global default per project.
    - Task-level Prompt Template can override both project and global defaults for a specific task.
    - Templates support variables: `{IssueNumber}`, `{Title}`, `{Description}`. The agent expands these before invoking Codex.
- Environment inheritance policy:
  - The agent does not sanitize or modify environment variables for the Codex process; it inherits the full calling environment as-is.
  - You can further control shell and sandbox policies via `codex/config.toml` (this repo includes a sample with `inherit = "all"`).

### Progressive Web App (PWA)

The client is configured as a PWA. A manifest.json and a service worker are included to enable installability and offline support. After running the app, install it from your browser's install prompt to use it in full-screen mode on desktop, tablet, or mobile.

#

### Task Dashboard

After starting the server, navigate to /dashboard in your browser to view the Task Dashboard page. It lists current and past AI tasks by fetching them from the `/api/tasks` endpoint and displays them using Tailwind CSS styling.

Important: The server no longer returns any preloaded or mock tasks. The list is empty until you create tasks via the UI or API. This ensures the dashboard reflects only user‑created work.

Tasks show their Priority and Deadline (when set). Each task displays a colored status badge (e.g., green for Closed, blue for Running, amber for Pending Review, gray for New/Queued, red for Failed) to improve scanability. The per‑task actions (View, Edit, Move, Close, Delete) are available under the three‑dot overflow menu on each item. The Close action appears only when the task is not already Closed and marks the task Closed immediately.

You can filter the list by priority and by status, and optionally sort by priority to bring the most important items to the top. When the number of tasks exceeds 50, pagination controls appear (50 items per page) with accessible Previous/Next buttons. The dashboard layout is responsive and keyboard-accessible.

### Task Details

Navigate to /tasks/{taskId} (for example, /tasks/00000000-0000-0000-0000-000000000000) in your browser to view the Task Details page. It fetches the task from /api/tasks/{taskId} and log data from /api/tasks/{taskId}/logs, rendering the logs in a terminal-style view without timestamp prefixes. The real‑time log is empty by default until the agent or server appends entries during processing. Additionally, a Start Codespace button appears when a task has a repository/branch context. Clicking it will list and start an existing GitHub Codespace (if present) or create and start a new codespace for the task, display real-time status and log updates, and provide an Open Codespace link and a Stop Codespace button to tear down the environment.

Codespace teardown behavior can be configured globally and per‑request:
- Global default: In server appsettings, set Codespaces:DeleteOnStop to true to delete codespaces when Stop is invoked, or false to simply stop them. The default is false.
- Per‑request override: On the Task Details page, a Delete on stop toggle appears when a codespace is active. When enabled, the Stop action will delete the codespace regardless of the global default; when disabled, it will only stop it. The server honors the request flag when present and falls back to the global setting otherwise.

Under the hood, the Task Details page establishes a SignalR connection to /hub/agent and invokes the JoinTaskGroup method with the task ID, ensuring that only updates for this task are received. It also handles connection errors and automatic reconnection—rejoining the task group if the connection is lost and displaying a warning in the log if real-time updates pause. When leaving the page, it calls LeaveTaskGroup to unsubscribe from further updates. The hub endpoint can be protected when application authentication is enabled on the server.

Troubleshooting “Start Codespace”
- The server prefers the signed-in user's GitHub OAuth token (stored in session) for all Codespaces lifecycle calls. Ensure your GitHub authorization includes the `codespace` scope (in addition to `repo` and `read:org`). If a session token is not available, the server falls back to `GitHub:Token` from configuration.
- When an existing Codespace is found for the repository, the server only calls Start when the Codespace state is `Stopped` or `Shutdown`. If the Codespace is already `Available` or `Starting`, the server skips the Start call and simply begins polling/logging state until it is available.
- You can configure the token in appsettings.json or using an environment variable, for example:
  - appsettings.Development.json
    { "GitHub": { "Token": "<your-PAT>" } }
  - Environment variable
    GitHub__Token=<your-PAT>


### Creating a New Task

On the Task Dashboard page, you can now create an arbitrary task with:
- Title, Created By, Due Date
- Priority (Low/Medium/High/Critical)
- Description (rich text)
- Custom Prompt (optional)

When you click Create Task, the task is added and you are redirected to the Task Details page. If you supplied a Custom Prompt, the agent will prefer that template when invoking Codex CLI. The template supports placeholders:
- {IssueNumber}, {Title}, {Description}

If no Custom Prompt is provided, the agent falls back to the project prompt (if set) or the global standard prompt, which includes the GitHub issue number when available.

Notes:
- Codespace controls on the Task Details page are available when a task has repository context (e.g., tasks imported from a linked project’s GitHub issues). The new‑task form on the dashboard does not capture repository/branch today.

### Project Dashboard

Navigate to /project-dashboard or click Project Dashboard in the navigation menu to open the Project Dashboard. From there, click the New Project button at the top to create a new project. The New Project button on the Projects page also routes here for a consistent creation flow.

## Agent Upgrade

The Project Details page includes an "Update Agent" button that:
- Fetches the latest release from `ReleasedGroup/master-control-program`.
- Updates or creates `scripts/mcp-agentclient-linux-x64` in the project's repository on a new branch.
- Opens a PR into the base branch containing the line `Closes #449`.

See docs/AgentUpgrade.md for details.

Note: The application no longer includes any default or sample projects. The Projects list is empty on first run until you create a project.

A creation form will appear allowing you to enter a project name and select one of your GitHub repositories from a searchable combobox. Only repositories that you have write access to (including those you collaborate on across organizations) are loaded via GitHub OAuth with full pagination support. When you are prompted by GitHub to grant repository access, choose All repositories (or select the specific repositories you need) so that all your repositories appear in the combobox. If repository loading fails (e.g., you are not signed in with GitHub), you can still type the repository identifier manually in the form as `owner/repo`, or click Login to GitHub and return. The selected repository identifier is then stored with the project in the application database. When a project is created, any open issues from the selected repository are imported as tasks. Import and Rediscover fetch all pages of open issues (per_page=100 with Link rel="next") so no issues are missed on larger repositories. You can optionally specify a GitHub IssueLabelFilter to only import issues with a particular label. Imported tasks record both the repository identifier and the GitHub issue number; this enables Codespace actions from the task page and prevents duplicate imports of the same issue if a project is re-imported or multiple projects target the same repository. Tasks are persisted to SQLite and survive logout and server restarts.

Project-level Prompt Template: From Project Details, click Edit to set an optional Prompt Template. This template is used by the agent when executing tasks in the project unless a task-level override is present. Supported variables: `{IssueNumber}`, `{Title}`, `{Description}`.

### Project Details

Navigate to /projects/{projectId} to view details for a specific project.

The page shows project metadata and tasks, and when you are signed in with GitHub it also displays the current sprint (GitHub milestone) summary for the linked repository:
- Active milestone name, due date, and progress (X of Y issues closed)
  
  Defaults for task dates:
  - Projects can optionally specify `DefaultTaskDueInDays` and `DefaultTaskDeadlineInDays` to control the default DueDate/Deadline for imported tasks (from GitHub issues).
  - When not set, the server uses 7 days from today for DueDate and aligns Deadline to the same date. The Dashboard new-task form also defaults the DueDate to 7 days out.
- Open issues assigned to the active milestone
- Backlog issues (open issues with no milestone)

Rediscover Tasks: Click the Rediscover Tasks button to resync the project’s tasks with GitHub issues. The server loads the repository’s current open issues with pagination, creates tasks for any new issues, and marks existing tasks as Closed if their linked GitHub issue is no longer open.

Task grouping: Tasks on Project Details are grouped by status in this order: Pending Review, Running, Queued, New, Closed. Each status renders as a section heading followed by its tasks. Any unexpected statuses appear afterward in alphabetical order.

### Mobile Navigation

On small screens, the left sidebar collapses into a hamburger menu in the header. Tap the hamburger to open the menu (the icon changes to an X). Tap the X or anywhere outside the panel to close it.

### UI Conventions

- Dropdown menus: use content-based sizing (`w-auto` with `min-w-max`) instead of fixed widths. This avoids arbitrary Tailwind spacing tokens (e.g., `w-32` vs `w-36`) and lets menus fit their labels without truncation or excessive whitespace. See docs/ui-guidelines.md for details and rationale.

## API Endpoints

The ASP.NET Core host exposes the following endpoints (see Program.cs for implementation):

- Tasks
- GET /api/tasks — list tasks
- GET /api/tasks/{id} — get a task by id
- POST /api/tasks — create a task (supports Title, CreatedBy, DueDate, Priority, Description, PromptTemplate, ProjectId, Repository, Branch)
  - GET /api/tasks/{id}/logs — get logs for a task
  - POST /api/tasks/{id}/logs — append a line to a task’s logs. Authorized when either:
    - user session is signed in with GitHub OAuth, or
    - request includes header `X-Project-Key` that matches the task’s project key (for headless agents)
    - dev/test resilience: when the database lacks a Projects entry for the task, the server falls back to in-memory project keys so agents can still append logs
  - PUT /api/tasks/{id}/status — update a task’s status (requires auth)
  - POST /api/tasks/{id}/cancel — request cancellation of a running task (requires auth)
  - POST /api/tasks/{id}/complete — agent callback to post final result (status, output, artifact links)
  - POST /api/tasks/{id}/codespace — start or create a Codespace for the task’s repo
  - POST /api/tasks/{id}/codespace/stop — stop a Codespace
  - Note: The start endpoint accepts the route with or without a GUID route constraint to improve reliability on certain hosting environments that could otherwise return 405 for POST when the constraint is misapplied.
- Projects
  - GET /api/projects — list projects (filtered by project-level access control)
  - GET /api/projects?all=1 — list all projects (for admin use)
  - GET /api/projects/{id} — get a project (enforces access control)
  - GET /api/projects/{id}/tasks — list tasks for a project
  - POST /api/projects/{id}/tasks/claim — atomically claim the next pending task for the project. Optional body { "agentId": "<guid>" }. Responses: 200 OK with TaskDto when a task is claimed; 204 No Content when no tasks are available; 404 Not Found if the project does not exist or the caller lacks access. Selection rules: the server considers `Queued` and `Pending` tasks first, and also allows claiming a task already marked `In Progress` if it is not yet assigned to an agent (AgentId is null). Note: In development and tests, if no task is found in the database, the server falls back to the in-memory task list to support scenarios that do not persist tasks. The agent client handles `204 No Content` gracefully and exits without error when no tasks are available.
  - GET /api/projects/{id}/key — download the project key
  - POST /api/projects — create a project (imports open GitHub issues into tasks if authenticated; optional IssueLabelFilter restricts imports to issues with the specified label; duplicate issues for the same repo/issue number are skipped)
  - PUT /api/projects/{id} — update a project
  - DELETE /api/projects/{id} — delete a project. All tasks associated with the project are deleted as part of the operation. Deletion is enforced both in the API (explicit removal) and at the database level (cascade delete on the Tasks → Projects foreign key).
  - GET /api/projects/{id}/access — list per-project user access entries
  - POST /api/projects/{id}/access — add a per-project user access entry
  - DELETE /api/projects/{id}/access/{accessId} — remove an access entry
- Clients
  - POST /api/clients — register a client agent
  - GET /api/clients — list clients
  - GET /api/clients/{id} — get a client by ID
- GitHub OAuth and repo listing
  - GET /login/github — begin OAuth flow
  - GET /login/github/callback — OAuth callback (stores token in session)
  - GET /logout — clear GitHub session token
  - GET /api/github/repos — list repos for the authenticated user
  - GET /api/ping — return session state (e.g., { "signedIn": true|false })
- SignalR hub
  - /hub/agent — real-time updates for task progress

- Settings
  - GET `/api/settings/standard-prompt` — get the global Standard Prompt template. Returns `200 OK` with `{ "value": "..." }` when set; returns `204 No Content` when the value is empty or not set.
  - PUT `/api/settings/standard-prompt` — set the global Standard Prompt using model binding. Body: `{ "value": "<template>" }`. Accepts `null` or empty strings, which are stored as empty and cause subsequent GETs to return `204 No Content`.
  - Notes: This endpoint uses a dedicated DTO (`StandardPromptDto`) and no longer manually reads/parses the request body stream.


### Project Access Control (Admin)

The Admin page includes a Project Access Control section that lets administrators:
- Assign users (by GitHub login or email) to specific projects
- Choose a role (Viewer or Contributor)
- Remove existing access entries

### Test Stability Notes

- Background task worker: The sequential processing test now waits until both enqueued tasks reach "Pending Review" before canceling the worker, instead of using a fixed cancel-after timer. This removes Release-mode flakiness in slower CI environments where cancellation could interrupt in-flight processing and yield a transient "Canceled" state.

Behavior:
- A project becomes restricted when it has one or more access entries; restricted projects are hidden from users without an explicit entry.
- Projects without any access entries remain visible to all users.
- All access checks are enforced server-side on project APIs.

Test coverage:
- Integration tests assert access behavior on these endpoints: `/api/projects/{id}`, `/api/projects/{id}/tasks`, `/api/projects/{id}/tasks/claim`, `/api/projects/{id}/key`, and `/api/projects/{id}/sprint`.
- When a project has no access entries, anonymous callers can access project-scoped endpoints. Note: `/api/projects/{id}/sprint` requires a GitHub session token and returns `401 Unauthorized` for anonymous callers, but is not hidden by `404`.
- When a project has access entries, anonymous callers receive `404 Not Found` (resource is hidden). Signed-in callers only succeed when their GitHub login or email matches a `ProjectAccess` entry; others receive `404`.

UI note:
- The `/admin` page has been simplified to show only the controls required for Project Access Control. Template/demo header, navigation, and profile placeholders were removed to reduce noise.

### Azure Active Directory Configuration (optional)

You can enable Azure AD authentication by registering the MCP application in your Azure AD tenant and configuring the ASP.NET Core host to use Microsoft Identity Web (OpenID Connect). This is optional and not required to run the sample locally. If you enable it, protect endpoints and the SignalR hub as desired and register authorization services on the Blazor client via AddAuthorizationCore() and a fallback AuthenticationStateProvider in src/MCP.Client/Program.cs to enable AuthorizeView.

## Changelog

See `docs/CHANGELOG.md` for notable changes and fixes.


## Architecture Overview

This section provides a high-level overview of the system architecture for the Master Control Program (MCP), outlining key decisions and main components.

## Key Decisions

### Blazor Hosting Model
The MCP frontend uses **Blazor WebAssembly (WASM)** hosted by an ASP.NET Core backend. The server hosts the static Blazor client (via UseBlazorFrameworkFiles) and exposes REST APIs and a SignalR hub that the client connects to for real-time updates. This model enables a lightweight client with PWA support while keeping integrations (GitHub OAuth, Codespaces, task orchestration) on the server.

Note: In the current demo configuration, the SignalR hub allows anonymous connections to ensure real-time updates work without a full authentication setup. Server APIs that mutate state continue to enforce session checks as appropriate. In production, configure authentication and add authorization to the hub as needed.

### Development Environment
Development and AI agent execution environments are provisioned using **GitHub Codespaces**. This ensures that developers and AI agents work in consistent, fully-configured devcontainer environments that match the production setup, improving reliability and reducing environment drift.

### Architecture Decision Records (ADRs)
Critical architectural decisions are recorded in ADRs to track the rationale, options considered, and outcomes. Where applicable, refer to individual ADR files for detailed decision logs.

## System Components

The MCP is structured as a modular monolithic ASP.NET Core application with the following core components:

- **Web UI (Blazor WebAssembly):** Provides the interactive user interface, styled with Tailwind CSS, and handles navigation and client-side state. Authentication (when enabled) is brokered by the ASP.NET Core host.
- **Task Orchestrator:** A backend service responsible for managing AI agent workflows, provisioning Codespaces, invoking OpenAI Codex API calls, and coordinating code generation tasks.
- **SignalR Hubs:** Facilitate real-time server-to-client communication for streaming task progress, logs, and notifications.
- **Progress Indicator UI:** A Tailwind-styled progress bar or stepper component in the Task Details page that visually represents the current task stage (Queued → Running → Completed), providing quick context alongside textual logs.
- **Integration Services:**
  - **OpenAI Codex Integration:** Handles prompt construction and interaction with the OpenAI API for code generation and analysis.
  - **GitHub API Integration:** Manages repository operations (fetching files, committing changes, creating pull requests) and Codespaces provisioning. It also exposes project‑level endpoints to surface GitHub data in the UI, such as the current sprint summary at GET /api/projects/{id}/sprint (active milestone info plus open issues in the sprint and backlog issues).
- **Background Services:** Hosted services that run asynchronous tasks (e.g., AI agent workflows) without blocking the main request pipeline.
- **Data Store:** Persistent storage (e.g., a relational database) for tracking projects, tasks, users, and system metadata.
- **Security & Configuration:** Uses Microsoft Entra ID (Azure AD) for authentication, Azure Key Vault for secret storage, and appsettings files / environment variables for configuration.

## Data Flow Overview

1. **User Action:** A user creates a task via the web UI or triggers a GitHub webhook.
2. **Orchestration:** The Task Orchestrator creates a new branch named `mcp-agent-{taskId}` from a configured base branch, provisions a Codespace on that branch, retrieves code context, and invokes the OpenAI Codex service. The base branch can be set via configuration (TaskProcessingOptions.BaseBranch). This value can now be overridden per project via the new Project settings (BaseBranch). If neither is configured, the task's requested branch is used when present; otherwise `main` is assumed. When creating the Codespace, a project‑level DevContainerPath may also be provided to select a non‑default devcontainer.
3. **Code Generation:** Responses from Codex are applied to the repository, tested, and committed via the GitHub API.
4. **Real-Time Updates:** SignalR streams task progress and results back to the Blazor UI.
5. **Completion:** The orchestrator creates a pull request from the `mcp-agent-{taskId}` branch for human review and cleans up the Codespace.

Refer to the detailed Technical Specification (technical.md) and individual ADR files for further implementation details.
