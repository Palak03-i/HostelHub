# UI redesign ("Aurora" theme)

Visual and motion changes only. No controllers, services, models, routes, forms or validation were modified.

## New files
- `wwwroot/css/modern.css` - theme layer loaded after `site.css`/`polish.css` (gradient palette, glass navbar, animated background, cards, buttons, tables, badges, alerts, modals, auth pages, home page).
- `wwwroot/js/motion.js` - scroll reveal, count-up numbers, progress-bar fill, card spotlight, button ripple, scroll progress bar, home hero tilt and word rotator. Respects `prefers-reduced-motion`; pages work if it fails to load.

## Edited files
- `Views/Shared/_Layout.cshtml` - links the font, `modern.css`, `motion.js` and adds the decorative background.
- `Views/Home/Index.cshtml` - new landing page (same Sign In / Student Registration links).
- All other views - only the old blue hex colors were swapped for the new indigo palette inside their inline `<style>` blocks.
- `wwwroot/css/site.css`, `polish.css` - same palette swap.

`wwwroot/js/enhancements.js` is untouched; the 9 frontend tests still pass.
