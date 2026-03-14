    using UnityEngine;

        public class SwingDirection : MonoBehaviour
        {
            public enum WeaponType {METAL = 1}
            Animator _anim;
            SpriteRenderer _sr;
            void Start()
            {
                _anim = GetComponent<Animator>();
                _sr = GetComponent<SpriteRenderer>();
            }
            public void Swing(WeaponType type)
            {
                _anim.SetTrigger("Swing");
                switch(type)
                {
                    case WeaponType.METAL:
                    _anim.SetInteger("Type", (int)type);
                    break;

                    default:
                    _anim.SetInteger("Type", 1);
                    break;
                }
            }
            public void SetSwingTransform(int dir, bool flipped)
            {
                switch(dir)
                {
                    case 1: //front
                    transform.localPosition = new Vector3(0, 0, -1);
                    transform.localRotation = Quaternion.Euler(-90f, 0, 180f);
                    _sr.flipX = !flipped;
                    break;

                    case 2: //right
                    transform.localPosition = new Vector3(1, 0, 0);
                    transform.localRotation = Quaternion.Euler(-90f, 0, 90f);
                    _sr.flipX = true;
                    break;

                    case 3: //back
                    transform.localPosition = new Vector3(0, 0, 1);
                    transform.localRotation = Quaternion.Euler(-90f, 0, 0);
                    _sr.flipX = flipped;
                    break;

                    case 4: //left
                    transform.localPosition = new Vector3(-1, 0, 0);
                    transform.localRotation = Quaternion.Euler(-90f, 0, -90f);
                    _sr.flipX = false;
                    break;
                }
            }
        }
