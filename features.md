Make a checklist of features to implement in the data table interface:

- right now, the side bar has a bottom bar with the dark mode toggle. The data area also has a fixed bar at the bottom. These two bars don't line up in height even though they are next to each other and fixed to the bottom. It looks wrong. Make the either make the pagination bar more compact, remove the top divider line above the dark mode toggle, or make the dark mode toggle section larger.
- The area in the side bar that says "Models" can also be an input that allows the user to filter the list of models. The placeholder of the input should be "Models".
- Remove the top part of the side bar where it shows the name of the app, label, and thumbnail.
- column type icon in the column header. (e.g. a calendar icon for date columns, a hash icon for number columns, etc.)
- checkbox column should be sticky/fixed when scrolling horizontally.
- when hovering over a row, show a tooltip with the full content of the cell if it is truncated.
- when hovering over a foreign key cell, show a button to jump to the related record in the referenced table.
- right now, at the end of each row, there is a "..." button that shows a dropdown menu with actions like "Edit", "Delete", etc. Remove this column.
- in the tab bar, before all the tabs, show an icon button that collapses or expands the side bar. This will give more horizontal space for the data table. If all tabs are closed, always show the side bar. The closing animation should look like the side bar is sliding in and out, not just disappearing and reappearing.
- in the data table, when reaching the last row, it doens't have a bottom border. Add a bottom border.
- I would like to add elevation to the side bar and the column headers, so that they visually pop out from the rest of the content. This will help to distinguish the different sections of the interface and make it look more polished. The column header only needs it on the bottom border.
- the checkbox (the first one) column should be sticky/fixed when scrolling horizontally.
- The tabs shouldn't show a row count.
- In the tab bar, all the tabs have some text and a close button. The text "Close All" button seems to be a different font. Make it match the font style of the rest fo the tabs.
- The Select Box for pagination that lets you select "Rows per page" is not wide enough to show the full text for 100. Make it a bit longer.
- Make sure the checkbox column is using the checkbox component from shadcn.
