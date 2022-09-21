using Cysharp.Threading.Tasks;

namespace Pspkurara.Sceneries.Dialogs {

	/// <summary>
	/// ダイアログ表示進捗ハンドラー。
	/// </summary>
	public sealed class OpenDialogHandler
	{

		/// <summary>
		/// ダイアログ表示進捗ハンドラーを生成。
		/// </summary>
		/// <param name="dialog">ダイアログのインスタンス</param>
		public OpenDialogHandler(Dialog dialog)
		{
			dialogInstance = dialog;
		}

		/// <summary>
		/// ダイアログインスタンスの本体。
		/// </summary>
		private readonly Dialog dialogInstance;

		/// <summary>
		/// 表示中であるか。
		/// </summary>
		public bool isOpening { get; internal set; }

		/// <summary>
		/// 外部から強制的に閉じる。
		/// </summary>
		public void ForceClose()
		{
			// インスタンスが残っていたら閉じさせる。
			if (dialogInstance)
			{
				dialogInstance.Close();
			}
		}

	}

}

