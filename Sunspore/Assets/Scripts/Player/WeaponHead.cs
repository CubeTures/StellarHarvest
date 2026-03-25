using System;
using Sunspore;
using UnityEditor.Build;
using UnityEngine;

        public class WeaponHead : MonoBehaviour
        {
            Animator _anim;
            SpriteRenderer _sr;
            HandHeld.WeaponHead head;
            void Awake()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
                SetSwingTransform(1, false);
            }
            public void TypeUpdate(HandHeld.WeaponHead newHead)
            {
                head = newHead;
                _anim.SetInteger("Type", (int)head);
            }
            public void Swing()
            {
                _anim.SetTrigger("Swing");
            }
            public void SetSwingTransform(int dir, bool flipped)
            {
                switch(dir)
                {
                    case 1: //front
                    transform.localPosition = new Vector3(0, -0.4f, -1);
                    transform.localRotation = Quaternion.Euler(-90f, 0, 180f);
                    _sr.flipX = !flipped;
                    _sr.sortingOrder = 1;
                    break;

                    case 2: //right
                    transform.localPosition = new Vector3(1, -0.4f, 0);
                    transform.localRotation = Quaternion.Euler(-90f, 0, 90);
                    _sr.flipX = true;
                    _sr.sortingOrder = 1;

                    break;

                    case 3: //back
                    transform.localPosition = new Vector3(0, -0.4f, 1);
                    transform.localRotation = Quaternion.Euler(-90, 0, 0);
                    _sr.flipX = flipped;
                    _sr.sortingOrder = -1;
                    break;

                    case 4: //left
                    transform.localPosition = new Vector3(-1, -0.4f, 0);
                    transform.localRotation = Quaternion.Euler(-90, 0, -90);
                    _sr.flipX = false;
                    _sr.sortingOrder = 1;
                    break;
                }
            }
        }
