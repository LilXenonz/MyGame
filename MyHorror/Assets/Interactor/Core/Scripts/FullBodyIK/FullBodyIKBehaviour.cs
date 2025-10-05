using UnityEngine;

namespace razz
{
	[HelpURL("https://negengames.com/interactor/components.html#fullbodyikbehaviourcs")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(InteractorIK))]
	public class FullBodyIKBehaviour : FullBodyIKBehaviourBase
	{
		[SerializeField] private FullBodyIK _fullBodyIK;

		public static FullBodyIKBehaviour Create(InteractorIK interactorIK)
		{
			FullBodyIKBehaviour fbikb = interactorIK.gameObject.GetComponent<FullBodyIKBehaviour>();
			if (!fbikb) fbikb = interactorIK.gameObject.AddComponent<FullBodyIKBehaviour>();

			fbikb.fullBodyIK.settings.animatorEnabled = FullBodyIK.AutomaticBool.Enable;
            fbikb.fullBodyIK.settings.resetTransforms = FullBodyIK.AutomaticBool.Disable;
            fbikb.fullBodyIK.settings.syncDisplacement = FullBodyIK.SyncDisplacement.Disable;

            for (int i = 0; i < fbikb.fullBodyIK.effectors.Length; i++)
            {
				fbikb.fullBodyIK.effectors[i].positionWeight = 0f;
			}

			Debug.Log("Full Body IK Behaviour component has been added to InteractorIK GameObject.", interactorIK.gameObject);
			return fbikb;
		}

		/*protected override void LateUpdate()
        {
#if UNITY_EDITOR
			if (!Application.isPlaying) return;
#endif

            if (!_interactorIK) _interactorIK = GetComponent<InteractorIK>();
            if (!_interactorIK || !_interactorIK.AnyInInteraction()) return;

            base.LateUpdate();
		}*/

		public override FullBodyIK fullBodyIK
		{
			get
			{
				if(_fullBodyIK == null) 
				{
					_fullBodyIK = new FullBodyIK();
					_fullBodyIK.Prefix(this.transform);
					_fullBodyIK.ConfigureBoneTransforms();
				}

				return _fullBodyIK;
			}
		}

		protected virtual void OnValidate()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying && InteractorIK.defaultFiles != 0)
			{
				UnityEditor.EditorApplication.delayCall += () =>
				{
					if (this != null)
					{
						Debug.Log("Interactor Full Body Component removed from Player because this is Final IK version.", this.gameObject);
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
						DestroyImmediate(this, true);
					}
				};
			}
#endif
		}
	}
}
