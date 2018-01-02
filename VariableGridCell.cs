using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
	[AddComponentMenu("Layout/Variable Grid Layout Group Cell", 140)]
	[RequireComponent(typeof(RectTransform))]
	[ExecuteInEditMode]
	public class VariableGridCell : UIBehaviour
	{
		[SerializeField]
		private bool m_OverrideForceExpandWidth = false;
		public virtual bool overrideForceExpandWidth {
			get { return m_OverrideForceExpandWidth; }
			set {
				if (value != m_OverrideForceExpandWidth) {
					m_OverrideForceExpandWidth = value;
					SetDirty ();
				}
			}
		}

		[SerializeField]
		private bool m_ForceExpandWidth = false;
		public virtual bool forceExpandWidth {
			get { return m_ForceExpandWidth; }
			set {
				if (value != m_ForceExpandWidth) {
					m_ForceExpandWidth = value;
					SetDirty ();
				}
			}
		}

		[SerializeField]
		private bool m_OverrideForceExpandHeight = false;
		public virtual bool overrideForceExpandHeight {
			get { return m_OverrideForceExpandHeight; }
			set {
				if (value != m_OverrideForceExpandHeight) {
					m_OverrideForceExpandHeight = value;
					SetDirty ();
				}
			}
		}

		[SerializeField]
		private bool m_ForceExpandHeight = false;
		public virtual bool forceExpandHeight {
			get { return m_ForceExpandHeight; }
			set {
				if (value != m_ForceExpandHeight) {
					m_ForceExpandHeight = value;
					SetDirty ();
				}
			}
		}

		[SerializeField]
		private bool m_OverrideCellAlignment = false;
		public virtual bool overrideCellAlignment {
			get { return m_OverrideCellAlignment; }
			set {
				if (value != m_OverrideCellAlignment) {
					m_OverrideCellAlignment = value;
					SetDirty ();
				}
			}
		}

		[SerializeField]
		private TextAnchor m_CellAlignment = TextAnchor.UpperLeft;
		public virtual TextAnchor cellAlignment {
			get { return m_CellAlignment; }
			set {
				if (value != m_CellAlignment) {
					m_CellAlignment = value;
					SetDirty ();
				}
			}
		}



		protected VariableGridCell()
		{}

		#region Unity Lifetime calls

		protected override void OnEnable()
		{
			base.OnEnable();
			SetDirty();
		}

		protected override void OnTransformParentChanged()
		{
			SetDirty();
		}

		protected override void OnDisable()
		{
			SetDirty();
			base.OnDisable();
		}

		protected override void OnDidApplyAnimationProperties()
		{
			SetDirty();
		}

		protected override void OnBeforeTransformParentChanged()
		{
			SetDirty();
		}

		#endregion

		protected void SetDirty()
		{
			if (!IsActive())
				return;
			LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
		}

		#if UNITY_EDITOR
		protected override void OnValidate()
		{
			SetDirty();
		}

		#endif
	}
}
