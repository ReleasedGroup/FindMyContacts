# Design Guidance
This document provides guidance on the design principles and best practices to be followed while developing the project. It includes information on UI/UX design, architectural patterns, and design patterns to ensure a consistent and high-quality design throughout the project.

# UI Elements

The following are examples of UI Elements that can be used in the project:

## Application Shells

Requirements
All of the components in Tailwind Plus are designed for the latest version of Tailwind CSS, which is currently Tailwind CSS v4.1. To make sure that you are on the latest version of Tailwind, update via npm:

npm install tailwindcss@latest
If you're new to Tailwind CSS, you'll want to read the Tailwind CSS documentation as well to get the most out of Tailwind Plus.

Add the Inter font family
We've used Inter for all of the Tailwind Plus examples because it's a beautiful font for UI design and is completely open-source and free. Using a custom font is nice because it allows us to make the components look the same on all browsers and operating systems.

You can use any font you want in your own project of course, but if you'd like to use Inter, the easiest way is to first add it via the CDN:

<link rel="stylesheet" href="https://rsms.me/inter/inter.css" />
Then add "InterVariable" to your "sans" font family in your Tailwind theme:

@theme {
  --font-sans: InterVariable, sans-serif;
  --font-sans--font-feature-settings: 'cv02', 'cv03', 'cv04', 'cv11';
}
If you're still on Tailwind CSS v3.x, you can do this in your tailwind.config.js file:

const defaultTheme = require('tailwindcss/defaultTheme')

module.exports = {
  theme: {
    extend: {
      fontFamily: {
        sans: ['InterVariable', ...defaultTheme.fontFamily.sans],
      },
    },
  },
  // ...
}
Dark mode support
If you're using dark mode components, add the dark:scheme-dark class to your <html> element to ensure that the browser renders scrollbars and other native UIs correctly in dark mode. Also include the dark:bg-gray-950 class to provide a dark background for the entire page:

<html class="bg-white dark:bg-gray-950 scheme-light dark:scheme-dark">

Using HTML
The HTML snippets in Tailwind Plus depend on a UI component library called Elements, which is used to power all of the interactive behavior, like dropdown menus, tabs, and listboxes.

Installing dependencies
A commercial license is required to use Tailwind Plus Elements.

The easiest way to install Elements is via the CDN. To do this, add the following script to your project's <head> tag:

<script src="https://cdn.jsdelivr.net/npm/@tailwindplus/elements@1" type="module"></script>
Alternatively, if you have a build pipeline you can also install it via npm:

npm install @tailwindplus/elements
Next, import Elements into your root layout:

import '@tailwindplus/elements';
See the Elements documentation for more information.

Creating components
Since the vanilla HTML examples included in Tailwind Plus can't take advantage of things like loops, there is a lot of repetition that wouldn't actually be there in a real-world project where the HTML was being generated from some dynamic data source. We might give you a list component with 5 list items for example that have all the utilities duplicated on each one, whereas in your project you'll actually be generating those list items by looping over an array.

When adapting our examples for your own projects, we recommend creating reusable template partials or JavaScript components as needed to manage any duplication.

Learn more about this in the "Using components" documentation on the Tailwind CSS website.