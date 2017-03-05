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

		[SerializeField] protected int m_ConstraintCount = 4;
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
		private Vector2[,] cellPreferredSizes;
		private float[] colPreferredWidths;
		private float[] rowPreferredHeights;

		//------------------------------------------------------------------------------------------------------
		private void InitializeLayout() {

			columns = (constraint == Constraint.FixedColumnCount) ? Mathf.Min(constraintCount, rectChildren.Count) : Mathf.CeilToInt((float)rectChildren.Count / (float)constraintCount);
			rows = (constraint == Constraint.FixedRowCount) ? Mathf.Min(constraintCount, rectChildren.Count) : Mathf.CeilToInt((float)rectChildren.Count / (float)constraintCount);

			cellIndexAtGridRef = new int[columns, rows];
			cellColumn = new int[rectChildren.Count];
			cellRow = new int[rectChildren.Count];
			cellPreferredSizes = new Vector2[columns, rows];
			colPreferredWidths = new float[rectChildren.Count];
			rowPreferredHeights = new float[rectChildren.Count];

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
				rNext = -1
			}
			int c = cOrigin;
			int r = rOrigin;

			for (int cell = 0; cell < rectChildren.Count; cell++) {
				cellIndexAtGridRef [c, r] = cell;
				cellColumn [cell] = c;
				cellRow [cell] = r;
				cellPreferredSizes [c, r] = new Vector2 (LayoutUtility.GetPreferredWidth, LayoutUtility.GetPreferredHeight);
				colPreferredWidths [c] = Mathf.Max (colPreferredWidths [c], cellPreferredSizes [c, r].x);
				rowPreferredHeights [c] = Mathf.Max (rowPreferredHeights [c], cellPreferredSizes [c, r].y);

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
						c += rNext;
					}
				}
			}
		}



		//------------------------------------------------------------------------------------------------------
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();

			int columns = GetColumnCount ();
			int rows = GetRowCount ();

			// Calc it based on all cells in that column
			// Cycle through all cells and store max values for column
			colMinWidths = new float[columns];
			colPreferredWidths = new float[columns];
//			colFlexibleWidths = new float[columns];
			for (int c = 0; c < columns; c++) {
				colMinWidths [c] = 0;
				colPreferredWidths [c] = 0;
//				colFlexibleWidths [c] = 0;
			}

			for (int i = 0; i < rectChildren.Count; i++)
			{
				var child = rectChildren [i];
				int col = GetCellColumn (i, columns, rows);
				colMinWidths[col] = Mathf.Max (colMinWidths[col], LayoutUtility.GetMinWidth (child));
				colPreferredWidths[col] = Mathf.Max (colPreferredWidths[col], LayoutUtility.GetPreferredWidth (child));
//				colFlexibleWidths[col] = Mathf.Max (colFlexibleWidths[col], LayoutUtility.GetFlexibleWidth (child));
			}

			float totalMinWidth = padding.horizontal;
			float totalPreferredWidth = padding.horizontal;
//			float totalFlexibleWidth = 0;
			for (int c = 0; c < columns; c++) {
				totalMinWidth += colMinWidths [c] + spacing.x;
				totalPreferredWidth += colPreferredWidths [c] + spacing.x;
//				totalFlexibleWidth += colFlexibleWidths [c] + spacing.x;
			}
			totalMinWidth -= spacing.x;
			totalPreferredWidth -= spacing.x;
//			totalFlexibleWidth -= spacing.x;

			SetLayoutInputForAxis (totalMinWidth, totalPreferredWidth, -1, 0);

			float prefW = LayoutUtility.GetPreferredWidth (rectTransform);
			float extraSpace = prefW - totalPreferredWidth;
			// Stretch if there is a layout element specifying extra space
			if (extraSpace > 0) {
				// Give extra space equally
				bool[] expandColumn = new bool[columns];
				int columnsToExpand = 0;

				for (int c = 0; c < columns; c++) {
					expandColumn [c] = false;
					for (int r = 0; r < rows; r++) {
						int index = GetCellIndexAtGridRef (c, r, columns, rows);
						if (index < rectChildren.Count) {
							var child = rectChildren [index];
							var cell = child.GetComponent<VGridCell> ();
							if (cell == null || cell.doNotExpandWidth == false) {
								expandColumn [c] = true;
								columnsToExpand++;
								break;
							}
						}
					}
				}

				for (int c = 0; c < columns; c++) {
					if (expandColumn[c])
						colPreferredWidths [c] += extraSpace / columnsToExpand;
				}
			}

//			Debug.Log (string.Format ("min {0} pref {1}", LayoutUtility.GetMinWidth(rectTransform), LayoutUtility.GetPreferredWidth(rectTransform)));
		}



		//------------------------------------------------------------------------------------------------------
		public int GetColumnCount() {
			return m_Constraint == Constraint.FixedColumnCount ? Mathf.Min(m_ConstraintCount, rectChildren.Count) : Mathf.CeilToInt((float)rectChildren.Count / (float)m_ConstraintCount);
		}

		//------------------------------------------------------------------------------------------------------
		public int GetRowCount() {
			return m_Constraint == Constraint.FixedRowCount ? Mathf.Min(m_ConstraintCount, rectChildren.Count) : Mathf.CeilToInt((float)rectChildren.Count / (float)m_ConstraintCount);
		}

		//------------------------------------------------------------------------------------------------------
		private int GetCellColumn(int cellIndex, int columns, int rows) {
			// Depends on start axis and start corner
			if (startCorner == Corner.UpperLeft || startCorner == Corner.LowerLeft) {
				if (startAxis == Axis.Horizontal) {
					return cellIndex % columns;
				} else {
					return Mathf.FloorToInt((float)cellIndex / (float)rows);
				}
			} else {
				if (startAxis == Axis.Horizontal) {
					return columns - 1 - (cellIndex % columns);
				} else {
					return columns - 1 - Mathf.FloorToInt((float)cellIndex / (float)rows);
				}
			}
		}

		//------------------------------------------------------------------------------------------------------
		private int GetCellRow(int cellIndex, int columns, int rows) {
			// Depends on start axis and start corner
			if (startCorner == Corner.UpperLeft || startCorner == Corner.UpperRight) {
				if (startAxis == Axis.Vertical) {
					return cellIndex % rows;
				} else {
					return Mathf.FloorToInt((float)cellIndex / (float)columns);
				}
			} else {
				if (startAxis == Axis.Vertical) {
					return rows - 1 - (cellIndex % rows);
				} else {
					return rows - 1 - Mathf.FloorToInt((float)cellIndex / (float)columns);
				}
			}
		}

		//------------------------------------------------------------------------------------------------------
		private int GetCellIndexAtGridRef( int x, int y, int columns, int rows ) {
			// Depends on start axis and start corner
			if (startCorner == Corner.UpperLeft) {
				if (startAxis == Axis.Horizontal)
					return y * columns + x;
				else
					return x * rows + y;
				
			} else if (startCorner == Corner.UpperRight) {
				if (startAxis == Axis.Horizontal)
					return y * columns + (columns - 1 - x);
				else
					return (columns - 1 - x) * rows + y;

			} else if (startCorner == Corner.LowerLeft) {
				if (startAxis == Axis.Horizontal)
					return (rows - 1 - y) * columns + x;
				else
					return x * rows + (rows - 1 - y);

			} else {
				if (startAxis == Axis.Horizontal)
					return (rows - 1 - y) * columns + (columns - 1 - x);
				else
					return (columns - 1 - x) * rows + (rows - 1 - y);
			}
		}

		//------------------------------------------------------------------------------------------------------
		public override void CalculateLayoutInputVertical()
		{
			int columns = GetColumnCount ();
			int rows = GetRowCount ();

			// Calc it based on all cells in that column
			// Cycle through all cells and store max values for column
			rowMinHeights = new float[rows];
			rowPreferredHeights = new float[rows];
//			rowFlexibleHeights = new float[rows];
			for (int r = 0; r < rows; r++) {
				rowMinHeights [r] = 0;
				rowPreferredHeights [r] = 0;
//				rowFlexibleHeights [r] = 0;
			}

			for (int i = 0; i < rectChildren.Count; i++)
			{
				var child = rectChildren [i];
				int row = GetCellRow (i, columns, rows);
				rowMinHeights[row] = Mathf.Max (rowMinHeights[row], LayoutUtility.GetMinHeight (child));
				rowPreferredHeights[row] = Mathf.Max (rowPreferredHeights[row], LayoutUtility.GetPreferredHeight (child));
//				rowFlexibleHeights[row] = Mathf.Max (rowFlexibleHeights[row], LayoutUtility.GetFlexibleHeight (child));
			}

			float totalMinHeight = padding.vertical;
			float totalPreferredHeight = padding.vertical;
			//			float totalFlexibleHeight = 0;
			for (int r = 0; r < rows; r++) {
				totalMinHeight += rowMinHeights [r] + spacing.y;
				totalPreferredHeight += rowPreferredHeights [r] + spacing.y;
				//				totalFlexibleHeight += rowFlexibleHeights [r] + spacing.y;
			}
			totalMinHeight -= spacing.y;
			totalPreferredHeight -= spacing.y;
			//			totalFlexibleHeight -= spacing.y;

			SetLayoutInputForAxis (totalMinHeight, totalPreferredHeight, -1, 1);

			float prefH = LayoutUtility.GetPreferredHeight (rectTransform);
			float extraSpace = prefH - totalPreferredHeight;
			// Stretch if there is a layout element specifying extra space
			if (extraSpace > 0) {
				// Give extra space equally
				for (int r = 0; r < rows; r++) {
					rowPreferredHeights [r] += extraSpace / rows;
				}
			}

		}

		//------------------------------------------------------------------------------------------------------
		public override void SetLayoutHorizontal()
		{
			SetCellsAlongAxis(0);
		}

		public override void SetLayoutVertical()
		{
			SetCellsAlongAxis(1);
		}

		private void SetCellsAlongAxis(int axis)
		{
			int columns = GetColumnCount ();
			int rows = GetRowCount ();
			int units = axis == 0 ? columns : rows;

			float[] size = new float[units];
			float[] pos = new float[units];

			// Get origin
			float space = axis == 0 ? rectTransform.rect.width : rectTransform.rect.height;
			float extraSpace = space - LayoutUtility.GetPreferredSize (rectTransform, axis);
			float origin = 0;
			if (axis == 0) {
				origin = padding.left;
				if (childAlignment == TextAnchor.UpperCenter || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.LowerCenter) {
					origin += extraSpace / 2f;
				}
				else if (childAlignment == TextAnchor.UpperRight || childAlignment == TextAnchor.MiddleRight || childAlignment == TextAnchor.LowerRight) {
					origin += extraSpace;
				}
			} else {
				origin = padding.top;
				if (childAlignment == TextAnchor.MiddleLeft || childAlignment == TextAnchor.MiddleCenter || childAlignment == TextAnchor.MiddleRight) {
					origin += extraSpace / 2f;
				}
				else if (childAlignment == TextAnchor.LowerLeft || childAlignment == TextAnchor.LowerCenter || childAlignment == TextAnchor.LowerRight) {
					origin += extraSpace;
				}
			}

			pos [0] = origin;
			size[0] = axis == 0 ? colPreferredWidths [0] : rowPreferredHeights [0];
			if (units > 1) {
				for (int u = 1; u < units; u++) {
					pos [u] = pos [u - 1] + size [u - 1] + (axis == 0 ? spacing.x : spacing.y);
					size[u] = axis == 0 ? colPreferredWidths [u] : rowPreferredHeights [u];
				}
			}

			for (int i = 0; i < rectChildren.Count; i++) {

				int index = axis == 0 ? GetCellColumn (i, columns, rows) : GetCellRow (i, columns, rows);

				SetChildAlongAxis (rectChildren [i], axis, pos[index], size[index]);
			}
		}
	}
}
