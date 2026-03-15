using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Sunspore;
using UnityEngine;

        public class WeaponHandle : MonoBehaviour
        {
            Weapon.WeaponHandle handle;
            Animator _anim;
            SpriteRenderer _sr;
            void Awake()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
            }

            public void TypeUpdate(Weapon.WeaponHandle newHandle)
            {
                handle = newHandle;
                _anim.SetInteger("Type", (int)handle);
            }
            public void Swing()
            {
                _anim.SetTrigger("Swing");
            }
            public void SetWeaponBaseTransform(int dir, bool flipped)
            {
                _sr.flipX = flipped;
                switch(dir)
                {
                    case 1: //front
                    _anim.SetInteger("Direction", dir);
                    break;

                    case 2: //right
                    _anim.SetInteger("Direction", dir);
                    break;

                    case 3: //back
                    _anim.SetInteger("Direction", dir);
                    break;

                    case 4: //left
                    _anim.SetInteger("Direction", dir);
                    break;
                }
            }
        }
