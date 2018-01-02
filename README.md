# VariableGridLayoutGroup

The built-in GridLayoutGroup component in Unity's UI is limited to identical cell sizes specified in the inspector. This custom script allows you to create a grid whose columns and rows are variable sizes, dynamically resizing to fit the largest content in that row or column.

An explainer video is hosted at: https://www.youtube.com/watch?v=m4a_WFMDB50

NB: If a cell contains a Text element which is set to wrap, the cell may become taller than needed. To get round this, it is best practice to make every cell a GameObject containing a Horizontal Layout Group, with Child Controls Size true and Child Force Expand false. Then attach a child to this cell object, and add the text element there instead. If desired, you can add a LayoutElement to the cell root object and set a preferred width/height.

NB: If you add a VariableGridCell element to a cell, you can override Force Expand etc. However, if you disable the VariableGridCell, you may need to disable and enable the GridLayoutGroup to refresh the layout.
