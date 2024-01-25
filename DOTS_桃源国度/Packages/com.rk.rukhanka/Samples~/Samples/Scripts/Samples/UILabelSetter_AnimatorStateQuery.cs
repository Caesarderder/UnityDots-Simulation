using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

/////////////////////////////////////////////////////////////////////////////////

namespace Rukhanka.Samples
{
class UILabelSetter_AnimatorStateQuery: MonoBehaviour
{
	public TextMeshProUGUI stateInfoLabel;
	public TextMeshProUGUI transitionInfoLabel;
	public TextMeshProUGUI sampleDescription;

	EntityManager em;
	EntityQuery animatorsQuery;

/////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		var worlds = World.All;
		foreach (var w in worlds)
		{
			if (RukhankaSystemsBootstrap.IsClientOrLocalSimulationWorld(w))
			{
				em = w.EntityManager;
				break;
			}
		}

		var ecb0 = new EntityQueryBuilder(Allocator.Temp)
		.WithAll<AnimatorControllerLayerComponent>();
		animatorsQuery = em.CreateEntityQuery(ecb0);

	#if !RUKHANKA_DEBUG_INFO
		sampleDescription.text += " Define <color=red>RUKHANKA_DEBUG_INFO</color> script symbol to view state and transition names.";
	#endif
	}

/////////////////////////////////////////////////////////////////////////////////

	void OnDestroy()
	{
	}

/////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		var animatedEntites = animatorsQuery.ToEntityArray(Allocator.Temp);
		if (animatedEntites.Length > 0)
		{
			var e = animatedEntites[0];
			var a = em.GetAspect<AnimatorStateQueryAspect>(e);
			
			var stateInfo = a.GetLayerCurrentStateInfo(0);
			var transitionInfo = a.GetLayerCurrentTransitionInfo(0);

		#if RUKHANKA_DEBUG_INFO
			stateInfoLabel.text = $"<color=green>Current State:</color> name <color=green>'{stateInfo.name}'</color>, hash <color=green>'{stateInfo.hash}'</color>, and time <color=#ff4500>'{stateInfo.normalizedTime:F2}'</color>";
		#else
			stateInfoLabel.text = $"<color=green>Current State</color>: hash <color=green>'{stateInfo.hash}'</color>, and time <color=#ff4500>'{stateInfo.normalizedTime:F2}'</color>";
		#endif

			if (transitionInfo.hash != 0)
			#if RUKHANKA_DEBUG_INFO
				transitionInfoLabel.text = $"<color=blue>Current Transition:</color> name <color=blue>'{transitionInfo.name}'</color>, hash <color=blue>'{transitionInfo.hash}'</color> and time <color=#ff4500>'{transitionInfo.normalizedTime:F2}'</color>";
			#else
				transitionInfoLabel.text = $"<color=blue>Current Transition:</color> hash <color=blue>'{transitionInfo.hash}'</color> and time <color=#ff4500>'{transitionInfo.normalizedTime:F2}'</color>";
			#endif
			else
				transitionInfoLabel.text = $"<color=blue>Not in transition</color>";

		}

	}
}
}
