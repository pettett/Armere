using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(menuName = "Channels/Save Load Event")]
public class SaveLoadEventChannel : SaveableSO
{
	public delegate void WriteAction(in GameDataWriter writer);
	public delegate void ReadAction(in GameDataReader writer);

	public event WriteAction onSaveBinEvent;
	public event ReadAction onLoadBinEvent;
	public event UnityAction onLoadBlankEvent;

	public override void SaveBin(in GameDataWriter writer)
	{
		onSaveBinEvent?.Invoke(in writer);
	}
	public override void LoadBin(in GameDataReader reader)
	{
		onLoadBinEvent?.Invoke(in reader);
	}
	public override void LoadBlank()
	{
		onLoadBlankEvent?.Invoke();
	}
}
