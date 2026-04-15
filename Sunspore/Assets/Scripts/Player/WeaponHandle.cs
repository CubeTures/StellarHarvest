using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Sunspore;
using UnityEngine;

        public class WeaponHandle : MonoBehaviour
        {
            HandHeld.WeaponHandle handle;
            Animator _anim;
            SpriteRenderer _sr;
            SpriteRenderer _playerSr;
            public SpriteRenderer headSr;

            int _currentDirection = -1;
            int sortOrder = 0;
            int playerSortOrder = 0;

            
            void Awake()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
                _playerSr = GetComponentInParent<SpriteRenderer>();

                var headTransform = transform.Find("WeaponHead");
                if (headTransform != null)
                {
                    headSr = headTransform.GetComponent<SpriteRenderer>();
                }

                if (headSr == null)
                {
                    var renderers = GetComponentsInChildren<SpriteRenderer>();
                    foreach (var renderer in renderers)
                    {
                        if (renderer != _sr)
                        {
                            headSr = renderer;
                            break;
                        }
                    }
                }
            }

            void Update()
            {
                sortOrder = _playerSr.sortingOrder;
                headSr.sortingOrder = sortOrder;
            }

            public void TypeUpdate(HandHeld.WeaponHandle newHandle)
            {
                handle = newHandle;
                _anim.SetInteger("Type", (int)handle);
            }

            public void SetHeadSprite(Sprite sprite)
            {
                if (headSr != null)
                {
                    headSr.sprite = sprite;
                }
            }
            public void Swing()
            {
                _anim.SetInteger("Direction", _currentDirection);
                _anim.SetTrigger("Swing");
            }
            public void SetWeaponBaseTransform(int dir, bool flipped)
            {
                /*_sr.flipX = flipped;
                _head.flipX = flipped;*/

                _anim.SetInteger("Direction", dir);
                _currentDirection = dir;

                if (dir == 3)
                {
                    sortOrder -= 100;
                }
                else
                {
                    sortOrder += 100;
                }
            }
        }
