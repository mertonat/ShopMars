using System.Collections;
using DG.Tweening;
using UnityEngine;

public class MoneyActor : MonoBehaviour
{
   
    [SerializeField] private GameObject model;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 rotate;
    [SerializeField] private int addMoney;
    [SerializeField] private Collider col;
    public void Move(Vector3 pos)
    {
        StartCoroutine(DelayMove());
        IEnumerator DelayMove()
        {
            col.enabled = false;
            transform.DOJump(pos,1f, 1, speed).SetEase(Ease.Linear);
            transform.DOLocalRotate(rotate, speed).SetEase(Ease.Linear);
            yield return new WaitForSeconds(speed);
            Destroy(this.gameObject);
        }
        
    }
    public void Move(Vector3 pos,Vector3 selectRotate)
    {
        col.enabled = false;
        transform.DOLocalJump(pos, 1f, 1, speed).SetEase(Ease.Linear);
        transform.DOLocalRotate(selectRotate, speed).SetEase(Ease.Linear);
    }
    public void MoveAdd(Vector3 pos)
    {
        StartCoroutine(DelayMove());
        IEnumerator DelayMove()
        {
            col.enabled = false;
            transform.DOJump(pos, 1f, 1, speed).SetEase(Ease.Linear);
            transform.DOLocalRotate(rotate, speed).SetEase(Ease.Linear);
            yield return new WaitForSeconds(speed);
            //CoreGameSignals.onAddMoney(addMoney);
            Destroy(this.gameObject);
        }

    }
}
