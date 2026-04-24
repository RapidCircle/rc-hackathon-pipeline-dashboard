<p align="center">
  <img src="../docs/assets/logo.jpg" alt="Rapidcircle Logo" height="80">
</p>

# Rapidcircle

**Website:** [https://rapidcircle.com](https://rapidcircle.com)

## About Us

Rapid Circle is a digital transformation and IT services company focused on AI, cloud, data, security and managed services, with deep specialisation in the Microsoft ecosystem (Microsoft 365, Azure, Copilot, Copilot Studio). They help organisations modernise infrastructure, upgrade legacy systems, build modern applications, secure their environments, improve ways of working, and deploy practical AI and Copilot-based business solutions. Their clients span sectors such as construction and manufacturing, education, energy and sustainability, government, healthcare, not‑for‑profit and retail, including large enterprises like Shell, Toyota and Suncorp. Rapid Circle positions itself as “large enough to deliver, small enough to care,” emphasising a people‑first, outcome‑driven approach, proven frameworks, and long‑term managed services to ensure continuity and continuous improvement. They highlight awards and Microsoft partner recognitions, including an official Microsoft Copilot Specialisation, as proof of their expertise.

## Brand Colors

Use these colors to maintain brand consistency throughout the application.

| Preview | Hex Code | CSS Variable Suggestion |
|---------|----------|------------------------|
| ![#0050A0](https://img.shields.io/badge/-0050A0-0050A0?style=flat-square) | `#0050A0` | `--brand-primary` |
| ![#00AEEF](https://img.shields.io/badge/-00AEEF-00AEEF?style=flat-square) | `#00AEEF` | `--brand-secondary` |
| ![#111827](https://img.shields.io/badge/-111827-111827?style=flat-square) | `#111827` | `--brand-accent` |
| ![#F15A24](https://img.shields.io/badge/-F15A24-F15A24?style=flat-square) | `#F15A24` | `--brand-highlight` |
| ![#FFFFFF](https://img.shields.io/badge/-FFFFFF-FFFFFF?style=flat-square) | `#FFFFFF` | `--brand-neutral` |

### CSS Variables

```css
:root {
  --brand-primary: #0050A0;
  --brand-secondary: #00AEEF;
  --brand-accent: #111827;
  --brand-highlight: #F15A24;
  --brand-neutral: #FFFFFF;
}
```

### Tailwind CDN Inline Config

Paste this `<script>` tag immediately after loading `/vendor/tailwindcss.js` on every HTML page.
This maps brand colors to Tailwind utility classes like `bg-primary`, `text-accent`, etc.

```html
<script>
  tailwind.config = {
    theme: {
      extend: {
        colors: {
          'primary': '#0050A0',
          'secondary': '#00AEEF',
          'accent': '#111827',
          'highlight': '#F15A24',
          'neutral': '#FFFFFF'
        }
      }
    }
  }
</script>
```

## Logo

![Rapidcircle Logo](../docs/assets/logo.jpg)

**Logo Path:** `docs/assets/logo.jpg`

---

*This document was auto-generated from the client's website. Please verify and update as needed.*
