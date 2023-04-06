using System;

namespace Zephyr {
    internal sealed class Union2<A, B> {
        readonly A Item1;
        readonly B Item2;
        int tag;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Union2(A item) { Item1 = item; tag = 0; }
        public Union2(B item) { Item2 = item; tag = 1; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void Match(Action<A> f, Action<B> g) {
            switch (tag) {
            case 0:
                f(Item1);
                break;
            case 1:
                g(Item2);
                break;
            default:
                throw new Exception("Unrecognized tag value: " + tag);
            }
        }
    }
}
