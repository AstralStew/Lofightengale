using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Paperticket;

public class ImageController : MonoBehaviour
{

    [Header("Controls")]
    
    [SerializeField] Image _Image;
    [SerializeField] Text _Text;

    float c;

    Coroutine activeTracker;

    void OnEnable() {

        CommandManager.onCommandRegistered += ActivateImage;

        _Image.color = new Color(1, 1, 1, 0);
        _Text.color = new Color(1, 0.8f, 0, 0);

    }

    public void ActivateImage(Command command) {

        StopAllCoroutines();

        //_Image.sprite = command.commandImage;
        _Text.text = command.commandName;

        StartCoroutine(ImageActive(command.recoveryLength));

    }


    IEnumerator ImageActive(int duration) {

        _Image.color = new Color(1, 1, 1, 1);
        _Text.color = new Color(1, 0.8f, 0, 1);

        // Wait for the recovery length
        int frame = duration;
        while (frame > 0) {
            yield return null;
            frame--;
        }

        _Image.color = new Color(1, 1, 1, 0);
        _Text.color = new Color(1, 0.8f, 0, 0);

    }

}
