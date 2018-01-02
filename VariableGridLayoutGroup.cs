using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[AddComponentMenu("Layout/Variable Grid Layout Group", 152)]
	public class VariableGridLayoutGroup : LayoutGroup
	{
		public enum Corner { UpperLeft = 0, UpperRight = 1, LowerLeft = 2, LowerRight = 3 }
		public enum Axis { Horizontal = 0, Vertical = 1 }
		public enum Constraint { FixedColumnCount = 0, FixedRowCount = 1 }

		[SerializeField] protected Corner m_StartCorner = Corner.UpperLeft;
		public Corner startCorner { get { return m_StartCorner; } set { SetProperty(ref m_StartCorner, value); } }

		[SerializeField] protected Axis m_StartAxis = Axis.Horizontal;
		public Axis startAxis { get { return m_StartAxis; } set { SetProperty(ref m_StartAxis, value); } }

		[SerializeField] protected TextAnchor m_CellAlignment = TextAnchor.UpperLeft;
		public TextAnchor cellAlignment { get { return m_CellAlignment; } set { SetProperty(ref m_CellAlignment, value); } }

		[SerializeField] protected Vector2 m_Spacing = Vector2.zero;
		public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

		[SerializeField] protected Constraint m_Constraint = Constraint.FixedColumnCount;
		public Constraint constraint { get { return m_Constraint; } set { SetProperty(ref m_Constraint, value); } }

		[SerializeField] protected int m_ConstraintCount = 3;
		public int constraintCount { get { return m_ConstraintCount; } set { SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); } }

		[SerializeField] protected bool m_ChildForceExpandWidth = true;
		public bool childForceExpandWidth { get { return m_ChildForceExpandWidth; } set { SetProperty(ref m_ChildForceExpandWidth, value); } }

		[SerializeField] protected bool m_ChildForceExpandHeight = true;
		public bool childForceExpandHeight { get { return m_ChildForceExpandHeight; } set { SetProperty(ref m_ChildForceExpandHeight, value); } }

		protected VariableGridLayoutGroup()
		{}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			constraintCount = constraintCount;
		}

		#endif

		//------------------------------------------------------------------------------------------------------
		public int columns { get; private set; }
		public int rows { get; private set; }
		private int[,] cellIndexAtGridRef;
		private int[] cellColumn;
		private int[] cellRow;
		private Vector2[] cellPreferredSizes;
		private float[] columnWidths;
		private float[] rowHeights;
		private float totalColumnWidth;
		private float totalRowHeight;

		//------------------------------------------------------------------------------------------------------
		public int GetCellIndexAtGridRef( int column, int row ) {
			if (column >= 0 && column < columns && row >= 0 && row < rows)
				return cellIndexAtGridRef [column, row];
			else
				return -1;
		}

		//------------------------------------------------------------------------------------------------------
		public int GetCellColumn( int cellIndex ) {
			if (cellIndex >= 0 && cellIndex < rectChildren.Count)
				return cellColumn [cellIndex];
			else
				return -1;
		}

		//------------------------------------------------------------------------------------------------------
		public int GetCellRow( int cellIndex ) {
			if (cellIndex >= 0 && cellIndex < rectChildren.Count)
				return cellRow [cellIndex];
			else
				return -1;
		}

		//------------------------------------------------------------------------------------------------------
		public float GetColumnPositionWithinGrid( int column ) {

			if (column <= 0 || column >= columns)
				return 0;

			float pos = 0;
			for (int c = 0; c < column; c++) {
				pos += GetColumnWidth (c) + spacing.x;
			}
			return pos;
		}

		//------------------------------------------------------------------------------------------------------
		public float GetRowPositionWithinGrid( int row ) {

			if (row <= 0 || row >= rows)
				return 0;

			float pos = 0;
			for (int r = 0; r < row; r++) {
				pos += GetRowHeight (r) + spacing.y;
			}
			return pos;
		}

		//------------------------------------------------------------------------------------------------------
		public float GetColumnWidth( int column ) {

			if (column < 0 || column >= columns)
				return 0;

			return columnWidths [column];
		}

		//------------------------------------------------------------------------------------------------------
		public float GetRowHeight( int row ) {

			if (row < 0 || row >= rows)
				return 0;

			return rowHeights [row];
		}

		//------------------------------------------------------------------------------------------------------
		private void InitializeLayout() {

			columns = (constraint == Constraint.FixedColumnCount) ? Mathf.Min(constraintCount, rectChildren.Count) : Mathf.CeilToInt((float)rectChildren.Count / (float)constraintCount);
			rows = (constraint == Constraint.FixedRowCount) ? Mathf.Min(constraintCount, rectChildren.Count) : Mathf.CeilToInt((float)rectChildren.Count / (float)constraintCount);

			cellIndexAtGridRef = new int[columns, rows];
			cellColumn = new int[rectChildren.Count];
			cellRow = new int[rectChildren.Count];
			cellPreferredSizes = new Vector2[rectChildren.Count];
			columnWidths = new float[columns];
			rowHeights = new float[rows];
			totalColumnWidth = 0;
			totalRowHeight = 0;
			for (int a = 0; a < columns; a++) {
				for (int b = 0; b < rows; b++) {
					cellIndexAtGridRef [a, b] = -1;
				}
			}

			int cOrigin = 0;
			int rOrigin = 0;
			int cNext = 1;
			int rNext = 1;
			if (startCorner == Corner.UpperRight || startCorner == Corner.LowerRight) {
				cOrigin = columns - 1;
				cNext = -1;
			}
			if (startCorner == Corner.LowerLeft || startCorner == Corner.LowerRight) {
				rOrigin = rows - 1;
				rNext = -1;
			}
			int c = cOrigin;
			int r = rOrigin;

			for (int cell = 0; cell < rectChildren.Count; cell++) {
				cellIndexAtGridRef [c, r] = cell;
				cellColumn [cell] = c;
				cellRow [cell] = r;
				cellPreferredSizes [cell] = new Vector2 (LayoutUtility.GetPreferredWidth(rectChildren[cell]), LayoutUtility.GetPreferredHeight(rectChildren[cell]));
				columnWidths [c] = Mathf.Max (columnWidths [c], cellPreferredSizes [cell].x);
				rowHeights [r] = Mathf.Max (rowHeights [r], cellPreferredSizes [cell].y);

				// next
				if (startAxis == Axis.Horizontal) {
					c += cNext;
					if (c < 0 || c >= columns) {
						c = cOrigin;
						r += rNext;
					}
				} else {
					r += rNext;
					if (r < 0 || r >= rows) {
						r = rOrigin;
						c += cNext;
					}
				}
			}

			for (int col = 0; col < columns; col++) {
				totalColumnWidth += columnWidths[col];
			}
			for (int row = 0; row < rows; row++) {
				totalRowHeight += rowHeights[row];
			}
		}



		//------------------------------------------------------------------------------------------------------
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();

			InitializeLayout ();

			float totalMinWidth = padding.horizontal;
			float totalPreferredWidth = padding.horizontal + totalColumnWidth + spacing.x * (columns - 1);

			SetLayoutInputForAxis (totalMinWidth, totalPreferredWidth, -1, 0);

			// Stretch if there is a layout element specifying extra space and child force expand width is on
			float extraWidth = LayoutUtility.GetPreferredWidth (rectTransform) - totalPreferredWidth;
			if (extraWidth > 0 && childForceExpandWidth) {

				// Don't expand column if all cells in column override expansion to false
				bool[] expandColumn = new bool[columns];
				int columnsToExpand = 0;
				for (int c = 0; c < columns; c++) {
					expandColumn [c] = false;
					for (int r = 0; r < rows; r++) {
						int index = GetCellIndexAtGridRef (c, r);
						if (index < rectChildren.Count) {
							var child = rectChildren [index];
							var cellOptions = child.GetComponent<VariableGridCell> ();
							if (cellOptions == null || !cellOptions.overrideForceExpandWidth || cellOptions.forceExpandWidth) {
								expandColumn [c] = true;
								columnsToExpand++;
								break;
							}
						}
					}
				}

				// Give extra space equally - for future version could also make option to give extra space proportionally
				for (int c = 0; c < columns; c++) {
					if (expandColumn[c])
						columnWidths [c] += extraWidth / columnsToExpand;
				}
			}
		}



		//------------------------------------------------------------------------------------------------------
		public override void CalculateLayoutInputVertical()
		{
			float totalMinHeight = padding.vertical;
			float totalPreferredHeight = padding.vertical + totalRowHeight + spacing.y * (rows - 1);

			SetLayoutInputForAxis (totalMinHeight, totalPreferredHeight, -1, 1);

			// Stretch if there is a layout element specifying extra space and child force expand height is on
			float extraHeight = LayoutUtility.GetPreferredHeight (rectTransform) - totalPreferredHeight;
			if (extraHeight > 0 && childForceExpandHeight) {

				// Don't expand column if all cells in column override expansion to false
				bool[] expandRow = new bool[rows];
				int rowsToExpand = 0;
				for (int r = 0; r < rows; r++) {
					expandRow [r] = false;
					for (int c = 0; c < columns; c++) {
						int index = GetCellIndexAtGridRef (c, r);
						if (index >= 0 && index < rectChildren.Count) {
							var child = rectChildren [index];
							var cellOptions = child.GetComponent<VariableGridCell> ();
							if (cellOptions == null || !cellOptions.overrideForceExpandHeight || cellOptions.forceExpandHeight) {
								expandRow [r] = true;
								rowsToExpand++;
								break;
							}
						} else {
							expandRow [r] = true;
							rowsToExpand++;
							break;
						}
					}
				}
					
				// Give extra space equally
				for (int r = 0; r < rows; r++) {
					if (expandRow [r]) {
						rowHeights [r] += extraHeight / rowsToExpand;
					}
				}
			}

		}

		//------------------------------------------------------------------------------------------------------
		public override void SetLayoutHorizontal()
		{
			SetCellsAlongAxis(0);
		}

		//------------------------------------------------------------------------------------------------------
		public override void SetLayoutVertical()
		{
			SetCellsAlongAxis(1);
		}

		//------------------------------------------------------------------------------------------------------
		private void SetCellsAlongAxis(int axis)
		{
			// Get origin
			float space = (axis == 0 ? rectTransform.rect.width : rectTransform.rect.height);
			float extraSpace = space - LayoutUtility.GetPreferredSize (rectTransform, axis);

			float gridOrigin = (axis == 0 ? padding.left : padding.top);
			if (axis == 0) {
				if (childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.LowerCenter) {
					gridOrigin += extraSpace / 2f;
				}
				else if (childAlignment == TextAnchor.UpperRight || childAlignment == TextAnchor.MiddleRight || childAlignment == TextAnchor.LowerRight) {
					gridOrigin += extraSpace;
				}
			} else {
				if (childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.MiddleRight) {
					gridOrigin += extraSpace / 2f;
				}
				else if (childAlignment == TextAnchor.LowerLeft || childAlignment == TextAnchor.LowerCenter || childAlignment == TextAnchor.LowerRight) {
					gridOrigin += extraSpace;
				}
			}

			// Expansion/alignment options
			bool forceExpand = (axis == 0) ? childForceExpandWidth : childForceExpandHeight;
			int alignment = 0;
			if (axis == 0) {
				if (cellAlignment == TextAnchor.UpperLeft || cellAlignment == TextAnchor.MiddleLeft || cellAlignment == TextAnchor.LowerLeft)
					alignment = -1;
				if (cellAlignment == TextAnchor.UpperRight || cellAlignment == TextAnchor.MiddleRight || cellAlignment == TextAnchor.LowerRight)
					alignment = 1;
			} else {
				if (cellAlignment == TextAnchor.UpperLeft || cellAlignment == TextAnchor.UpperCenter || cellAlignment == TextAnchor.UpperRight)
					alignment = -1;
				if (cellAlignment == TextAnchor.LowerLeft || cellAlignment == TextAnchor.LowerCenter || cellAlignment == TextAnchor.LowerRight)
					alignment = 1;
			}

			// Set cells
			for (int i = 0; i < rectChildren.Count; i++) {

				int colrow = (axis == 0 ? GetCellColumn (i) : GetCellRow (i));

				// Column/row origin
				float cellOrigin = gridOrigin + (axis == 0 ? GetColumnPositionWithinGrid(colrow) : GetRowPositionWithinGrid(colrow));

				// Column/row size and space
				float cellSpace = (axis == 0 ? GetColumnWidth(colrow) : GetRowHeight(colrow));
				var child = rectChildren[i];
				float cellSize = LayoutUtility.GetPreferredSize(child,axis);
				float cellExtraSpace = cellSpace - cellSize;

				// If cell should stretch, place there. If not, place within cell space according to cell alignment and its preferred size
				bool cellForceExpand = forceExpand;
				int thisCellAlignment = alignment;
				var cellOptions = child.GetComponent<VariableGridCell> ();
				if (cellOptions != null) {
					if (axis == 0 ? cellOptions.overrideForceExpandWidth : cellOptions.overrideForceExpandHeight)
						cellForceExpand = (axis == 0 ? cellOptions.forceExpandWidth : cellOptions.forceExpandHeight);
					if (cellOptions.overrideCellAlignment) {
						if (axis == 0) {
							if (cellOptions.cellAlignment == TextAnchor.UpperLeft || cellOptions.cellAlignment == TextAnchor.MiddleLeft || cellOptions.cellAlignment == TextAnchor.LowerLeft)
								thisCellAlignment = -1;
							else if (cellOptions.cellAlignment == TextAnchor.UpperCenter || cellOptions.cellAlignment == TextAnchor.MiddleCenter || cellOptions.cellAlignment == TextAnchor.LowerCenter)
								thisCellAlignment = 0;
							else
								thisCellAlignment = 1;
						} else {
							if (cellOptions.cellAlignment == TextAnchor.UpperLeft || cellOptions.cellAlignment == TextAnchor.UpperCenter || cellOptions.cellAlignment == TextAnchor.UpperRight)
								thisCellAlignment = -1;
							else if (cellOptions.cellAlignment == TextAnchor.MiddleLeft || cellOptions.cellAlignment == TextAnchor.MiddleCenter || cellOptions.cellAlignment == TextAnchor.MiddleRight)
								thisCellAlignment = 0;
							else
								thisCellAlignment = 1;
						}
					}
				}
				if (cellForceExpand) {
					cellSize = cellSpace;
				} else {
					if (thisCellAlignment == 0)
						cellOrigin += cellExtraSpace / 2f;
					if (thisCellAlignment == 1)
						cellOrigin += cellExtraSpace;
				}

				SetChildAlongAxis (rectChildren [i], axis, cellOrigin, cellSize);
			}
		}
	}
}
