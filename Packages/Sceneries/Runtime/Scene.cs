using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using Pspkurara.Sceneries.Transitions;

namespace Pspkurara.Sceneries
{

	/// <summary>
	/// 各画面のインスタンス。
	/// </summary>
	[DisallowMultipleComponent]
	[RequireComponent(typeof(GraphicRaycaster))]
	[RequireComponent(typeof(Canvas))]
	public abstract class Scene : UIBehaviour
	{

		/// <summary>
		/// 画面が最後にアクティブになったタイミングの時間。
		/// <see cref="System.DateTime.UtcNow"/>が入る。
		/// </summary>
		internal System.DateTime lastActivatedTime { get; private set; }

		/// <summary>
		/// 自身の<see cref="GraphicRaycaster"/>。
		/// <see cref="InstancedSetup"/>で取得される。
		/// </summary>
		protected GraphicRaycaster graphicRaycaster { get; private set; }

		/// <summary>
		/// 自身の<see cref="Canvas"/>。
		/// <see cref="InstancedSetup"/>で取得される。
		/// </summary>
		protected Canvas canvas { get; private set; }

		/// <summary>
		/// 自身の<see cref="Canvas.rootCanvas"/>。
		/// <see cref="InstancedSetup"/>では<see cref="Canvas.rootCanvas"/>>が取得できないため、ここから取得する。
		/// </summary>
		protected Canvas rootCanvas
		{
			get
			{
				var canvasEnabled = canvas.enabled;
				canvas.enabled = true;
				var rootCanvas = canvas.rootCanvas;
				canvas.enabled = canvasEnabled;
				return rootCanvas;
			}
		}

		/// <summary>
		/// 画面遷移アニメーション。
		/// 任意アタッチ。
		/// </summary>
		protected ISceneTransition transition { get; private set; }

		#region virtual functions

		/// <summary>
		/// 画面インスタンス生成直後に呼び出される処理。
		/// </summary>
		protected virtual UniTask OnInstancedScene(){ return UniTask.CompletedTask; }

		/// <summary>
		/// 画面遷移開始時直前に呼び出される処理。
		/// これからこの画面になる場合向け。
		/// </summary>
		protected virtual UniTask OnBeforeStartInvationScene() { return UniTask.CompletedTask; }

		/// <summary>
		/// 画面遷移開始時に呼び出される処理。
		/// これからこの画面になる場合向け。
		/// </summary>
		protected virtual void OnStartInvationScene(){}

		/// <summary>
		/// 画面遷移完了時に呼び出される処理。
		/// これからこの画面になる場合向け。
		/// </summary>
		protected virtual void OnCompletedInvationScene(){}

		/// <summary>
		/// 画面遷移開始時に呼び出される処理。
		/// 現在この画面の場合向け。
		/// </summary>
		protected virtual void OnStartDepartureScene(){}

		/// <summary>
		/// 画面遷移完了時に呼び出される処理。
		/// 現在この画面の場合向け。
		/// </summary>
		protected virtual void OnCompletedDepartureScene(){}

		#endregion

		/// <summary>
		/// 生成時初期化処理を実行する。
		/// </summary>
		internal async UniTask InstancedSetup()
		{
			graphicRaycaster = GetComponent<GraphicRaycaster>();
			canvas = GetComponent<Canvas>();
			transition = GetComponent<ISceneTransition>();

			// 必ず初期化時は触れられないようにしておく
			graphicRaycaster.enabled = false;

			// 最初は非表示にしておく
			canvas.enabled = false;

			// 生成時に呼ばれる処理。
			await OnInstancedScene();
		}


		/// <summary>
		/// 画面のアクティブを切り替える。
		/// </summary>
		/// <param name="preActiveScene">直前にアクティブだった画面インスタンス</param>
		/// <param name="handler">マネージャーから渡されるデータハンドラー</param>
		internal async UniTask SetSceneActive(Scene preActiveScene, ManagerToSceneHandler handler)
		{
			// タッチ不可にしておく
			graphicRaycaster.enabled = false;
			// 画面を一番手前に移動
			transform.SetAsLastSibling();
			// アクティブにする
			gameObject.SetActive(true);
			// アクティブになった時間を格納
			lastActivatedTime = System.DateTime.UtcNow;

			await OnBeforeStartInvationScene();

			// 遷移開始処理を呼ぶ
			OnStartInvationScene();

			// 自身の画面遷移アニメーションが存在するか
			bool hasActiveSceneTween = transition != null;

			// 直前の画面が存在
			if (preActiveScene)
			{
				if (handler.IsMaintainOrder && handler.IsActiveScreenIsUnder)
				{
					// 古い画面を表側に持ってくる。
					preActiveScene.transform.SetAsLastSibling();
				}
				else
				{
					// 古い画面を裏側に持ってくる (新しい画面は表になる)。
					preActiveScene.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
				}

				// 直前の画面のタッチをオフ
				preActiveScene.graphicRaycaster.enabled = false;

				// 遷移開始処理を呼ぶ
				preActiveScene.OnStartDepartureScene();

				// レイアウト用に1フレーム待機
				await UniTask.Yield();

				// 直前の画面の画面遷移アニメーションが存在するか
				bool hasPreActiveSceneTween = preActiveScene.transition != null;

				// アニメーションを待つ
				// 裏側の画面のアニメーションは再生しない場合
				if (handler.IsSkipUnderUndoAnimation)
				{
					canvas.enabled = true;
					// ソート順が生成順の場合
					if (handler.IsMaintainOrder)
					{
						// アクティブ画面が裏側にある場合
						if (handler.IsActiveScreenIsUnder)
						{
							// 即時反映
							if (hasActiveSceneTween) transition.ImmediateApplyDepartureAnimation();
							// 直前の画面 (表側) が抜けていくアニメーションのみ再生すればよい
							if (hasPreActiveSceneTween) await preActiveScene.transition.PlayDepartureAnimationAndWaitComplete();
						}
						else
						{
							// 即時反映
							if (hasPreActiveSceneTween) preActiveScene.transition.ImmediateApplyDepartureAnimation();
							// アクティブ画面 (表側) が入ってくるアニメーションのみ再生すればよい
							if (hasActiveSceneTween) await transition.PlayInvationAnimationAndWaitComplete();
						}
					}
					// 必ずアクティブ画面が上に来る場合
					else
					{
						// アクティブ画面 (表側) が入ってくるアニメーションのみ再生すればよい
						if (hasActiveSceneTween) await transition.PlayInvationAnimationAndWaitComplete();
					}
				}
				// 通常のアニメーション
				else
				{
					// 両方の画面遷移アニメーションが可能
					if (hasActiveSceneTween && hasPreActiveSceneTween)
					{
						// 両方の画面で同時再生が許可されているときのみ
						if (transition.allowDuplicatePlayback && preActiveScene.transition.allowDuplicatePlayback)
						{
							// どちらのアニメーションも同時に再生
							canvas.enabled = true;
							await UniTask.WhenAll(
								transition.PlayInvationAnimationAndWaitComplete(),
								preActiveScene.transition.PlayDepartureAnimationAndWaitComplete()
								);
						}
						else
						{
							// 順に再生
							await preActiveScene.transition.PlayDepartureAnimationAndWaitComplete();
							canvas.enabled = true;
							await transition.PlayInvationAnimationAndWaitComplete();
						}
					}
					// アクティブ側の画面遷移アニメーションのみ可能
					else if (hasActiveSceneTween)
					{
						canvas.enabled = true;
						await transition.PlayInvationAnimationAndWaitComplete();
					}
					// 直前の画面の画面遷移アニメーションのみ可能
					else if (hasPreActiveSceneTween)
					{
						canvas.enabled = true;
						await preActiveScene.transition.PlayDepartureAnimationAndWaitComplete();
					}
				}

				// 相手が死んでいたら何もしない
				if (!preActiveScene.IsDestroyed())
				{
					// 完了処理を呼ぶ
					preActiveScene.OnCompletedDepartureScene();
					preActiveScene.canvas.enabled = false;
					preActiveScene.gameObject.SetActive(false);
				}
			}
			else
			{
				canvas.enabled = true;
				if (hasActiveSceneTween)
				{
					await transition.PlayInvationAnimationAndWaitComplete();
				}

			}

			// 自身が死んでいたら何もしない
			if (this.IsDestroyed()) return;

			// 完了処理を呼ぶ
			OnCompletedInvationScene();
			graphicRaycaster.enabled = true;
		}

	}

}
