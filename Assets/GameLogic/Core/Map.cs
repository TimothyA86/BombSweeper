using UnityEngine;
using System.Collections.Generic;

namespace Assets.GameLogic.Core
{
    public class Map : MonoBehaviour
    {
        public struct Cell
        {
            public int col;
            public int row;
        }

        private const float BombFuseTime = 0.25f;
        private const byte CoveredFlag = 0x10;
        private const byte ContainsBombFlag = 0x20;
        private const byte FlaggedFlag = 0x40;
        private const byte NearByBombsMask = 0x0F;

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private Transform bombParent;
        [SerializeField] private Transform coverParent;
        [SerializeField] private GameObject numberPrefab;
        [SerializeField] private Material[] numberMaterials;

        private byte[] cells;
        private MeshRenderer[] numbers;
        private Cover[] covers;
        private Dictionary<int, Bomb> bombs = new Dictionary<int, Bomb>();
        private List<int> pendingUncover = new List<int>(32);
        private Queue<Cell> cellQueue = new Queue<Cell>(32);

        private Transform numberParent;

        private void Awake()
        {
            cells = new byte[width * height];
            covers = new Cover[width * height];
            numbers = new MeshRenderer[width * height];

            numberParent = new GameObject("Numbers").transform;
            numberParent.parent = transform;
            numberParent.localPosition = Vector3.zero;

            SetBombData(bombParent.GetComponentsInChildren<Bomb>());
            SetCoverData(coverParent.GetComponentsInChildren<Cover>());
        }

        private void OnEnable()
        {
            Cover.CoverLeftClicked += OnCoverLeftClicked;
            Cover.CoverRightClicked += OnCoverRightClicked;
            Bomb.Exploded += OnBombExploded;
        }

        private void OnDisable()
        {
            Cover.CoverLeftClicked -= OnCoverLeftClicked;
            Cover.CoverRightClicked -= OnCoverRightClicked;
            Bomb.Exploded -= OnBombExploded;
        }

        private void OnCoverLeftClicked(Cover cover)
        {
            int col = (int)cover.transform.localPosition.x;
            int row = -(int)cover.transform.localPosition.z;

            if (InBounds(col, row) && !IsFlagged(col, row))
            {
                ScanfillUncover(col, row);
                UncoverPending();
            }
        }

        private void OnCoverRightClicked(Cover cover)
        {
            int col = (int)cover.transform.localPosition.x;
            int row = -(int)cover.transform.localPosition.z;

            if (InBounds(col, row))
            {
                ToggleFlag(col, row);
                cover.ShowFlag(IsFlagged(col, row));
            }
        }

        private void OnBombExploded(Bomb bomb)
        {
            int col = (int)bomb.transform.localPosition.x;
            int row = -(int)bomb.transform.localPosition.z;

            for (int r = row - 1; r <= row + 1; ++r)
            {
                for (int c = col - 1; c <= col + 1; ++c)
                {
                    if (InBounds(c, r))
                    {
                        int index = c + r * width;
                        byte data = GetData(index);

                        if (ContainsBomb(data))
                        {
                            TryExplodeBomb(index);
                        }

                        if (IsCovered(data))
                        {
                            Uncover(index);
                        }

                        UpdateNearByBombs(c, r, -1);
                    }
                }
            }

            UncoverPending();
        }

        private void SetBombData(Bomb[] bombParent)
        {
            foreach (var bomb in bombParent)
            {
                int col = (int)bomb.transform.localPosition.x;
                int row = -(int)bomb.transform.localPosition.z;

                if (InBounds(col, row))
                {
                    int index = col + row * width;
                    bombs.Add(index, bomb);
                    cells[index] |= ContainsBombFlag;
                    UpdateAreaNearByBombs(col, row, 1);
                }
            }
        }

        private void SetCoverData(Cover[] coverParent)
        {
            foreach (var cover in coverParent)
            {
                int col = (int)cover.transform.localPosition.x;
                int row = -(int)cover.transform.localPosition.z;

                if (InBounds(col, row))
                {
                    cells[col + row * width] |= CoveredFlag;
                    covers[col + row * width] = cover;
                }
            }
        }

        private void UpdateAreaNearByBombs(int col, int row, int delta)
        {
            for (int r = row - 1; r <= row + 1; ++r)
            {
                for (int c = col - 1; c <= col + 1; ++c)
                {
                    if (InBounds(c, r))
                    {
                        UpdateNearByBombs(c, r, delta);
                    }
                }
            }
        }

        private void UpdateNearByBombs(int col, int row, int delta)
        {
            int index = col + row * width;
            byte data = cells[index];
            int count = (data & NearByBombsMask) + delta;

            cells[index] = (byte)((data & ~NearByBombsMask) | (count & NearByBombsMask));

            if (count > 0 && !ContainsBomb(data))
            {
                var number = numbers[index];

                if (number == null)
                {
                    var gameObject = Instantiate(numberPrefab, numberParent);
                    gameObject.transform.localPosition = new Vector3(col, 0, -row);
                    number = gameObject.GetComponentInChildren<MeshRenderer>();
                    numbers[index] = number;
                }

                number.material = numberMaterials[count - 1];
            }
            else
            {
                var number = numbers[index];

                if (number != null)
                {
                    numbers[index] = null;
                    Destroy(number.transform.parent.gameObject);
                }
            }
        }

        private void TryExplodeBomb(int index)
        {
            Bomb bomb;

            if (bombs.TryGetValue(index, out bomb))
            {
                int mask = ~ContainsBombFlag;
                cells[index] &= (byte)(mask);
                bombs.Remove(index);
                bomb.Explode(BombFuseTime);
            }
        }

        private void ScanfillUncover(int col, int row)
        {
            int index = col + row * width;
            byte data = GetData(index);

            if (!IsCovered(data))
            {
                return;
            }

            if (NearByBombs(data) > 0)
            {
                if (ContainsBomb(data))
                {
                    TryExplodeBomb(index);
                }

                Uncover(index);
                return; // does not meet scan fill requirements
            }

            var queue = cellQueue;
            queue.Clear(); // just in case there is something left in the queue

            Cell cell;
            cell.col = col;
            cell.row = row;
            queue.Enqueue(cell);

            while (queue.Count > 0)
            {
                cell = queue.Dequeue();
                col = cell.col;
                row = cell.row;

                // Move as far left as possible before starting scan
                while (--col >= 0)
                {
                    index = col + row * width;
                    data = GetData(index);

                    if (IsFlagged(data))
                    {
                        break;
                    }

                    if (NearByBombs(data) > 0)
                    {
                        if (IsCovered(data))
                        {
                            Uncover(index);
                        }

                        // Uncover corners
                        if (row > 0 && NearByBombs(col + 1, row - 1) > 0)
                        {
                            if (IsCovered(col, row -1))
                            {
                                if (NearByBombs(col, row - 1) > 0)
                                {
                                    Uncover(col, row - 1);
                                }
                                else
                                {
                                    cell.col = col;
                                    cell.row = row - 1;
                                    queue.Enqueue(cell);
                                }
                            }
                        }

                        if (row + 1 < height && NearByBombs(col + 1, row + 1) > 0)
                        {
                            if (IsCovered(col, row + 1))
                            {
                                if (NearByBombs(col, row + 1) > 0)
                                {
                                    Uncover(col, row + 1);
                                }
                                else
                                {
                                    cell.col = col;
                                    cell.row = row + 1;
                                    queue.Enqueue(cell);
                                }
                            }
                        }

                        break;
                    }

                    if (!IsCovered(data))
                    {
                        break;
                    }
                }

                ++col;

                // Scan to the right, check above and below for line breaks
                bool aboveOk = false;
                bool belowOk = false;
                
                while (col < width && IsCoveredAndNotFlagged(col, row))
                {
                    Uncover(col, row);

                    if (row > 0)
                    {
                        if (aboveOk)
                        {
                            data = GetData(col, row - 1);
                            aboveOk = IsCoveredAndNotFlaggedAndNoBombsNear(data);
                        }
                        else
                        {
                            index = col + (row - 1) * width;
                            data = GetData(index);

                            if (IsCoveredAndNotFlagged(data))
                            {
                                if (NearByBombs(data) > 0)
                                {
                                    Uncover(index);
                                }
                                else
                                {
                                    cell.col = col;
                                    cell.row = row - 1;
                                    queue.Enqueue(cell);
                                    aboveOk = true;
                                }
                            }
                        }
                    }

                    if (row + 1 < height)
                    {
                        if (belowOk)
                        {
                            data = GetData(col, row + 1);
                            belowOk = IsCoveredAndNotFlaggedAndNoBombsNear(data);
                        }
                        else
                        {
                            index = col + (row + 1) * width;
                            data = GetData(index);

                            if (IsCoveredAndNotFlagged(data))
                            {
                                if (NearByBombs(data) > 0)
                                {
                                    Uncover(index);
                                }
                                else
                                {
                                    cell.col = col;
                                    cell.row = row + 1;
                                    queue.Enqueue(cell);
                                    belowOk = true;
                                }
                            }
                        }
                    }

                    if (NearByBombs(col, row) > 0)
                    {
                        break;
                    }

                    ++col;
                }
            }
        }

        private void UncoverPending()
        {
            foreach (int index in pendingUncover)
            {
                covers[index].gameObject.SetActive(false);
            }

            pendingUncover.Clear();
        }

        private void Uncover(int col, int row)
        {
            Uncover(col + row * width);
        }

        private void Uncover(int index)
        {
            int mask = ~CoveredFlag;
            cells[index] &= (byte)(mask);
            pendingUncover.Add(index);
        }

        private void ToggleFlag(int col, int row)
        {
            cells[col + row * width] ^= FlaggedFlag;
        }

        private bool IsCoveredAndNotFlagged(int col, int row)
        {
            return IsCoveredAndNotFlagged(GetData(col + row * width));
        }

        private bool IsCoveredAndNotFlagged(byte data)
        {
            return ((data & (CoveredFlag | FlaggedFlag)) == CoveredFlag);
        }

        private bool IsCoveredAndNotFlaggedAndNoBombsNear(int col, int row)
        {
            return IsCoveredAndNotFlaggedAndNoBombsNear(GetData(col + row * width));
        }

        private bool IsCoveredAndNotFlaggedAndNoBombsNear(byte data)
        {
            return ((data & (CoveredFlag | FlaggedFlag | NearByBombsMask)) == CoveredFlag);
        }

        private byte GetData(int index)
        {
            return cells[index];
        }

        public byte GetData(int col, int row)
        {
            return GetData(col + row * width);
        }

        public bool InBounds(int col, int row)
        {
            return (col >= 0 && col < width && row >= 0 && row < height);
        }

        public bool IsCovered(int col, int row)
        {
            return IsCovered(cells[col + row * width]);
        }

        public bool IsCovered(byte data)
        {
            return ((data & CoveredFlag) != 0);
        }

        private bool IsFlagged(int col, int row)
        {
            return IsFlagged(cells[col + row * width]);
        }

        private bool IsFlagged(byte data)
        {
            return ((data & FlaggedFlag) != 0);
        }

        public bool ContainsBomb(int col, int row)
        {
            return ContainsBomb(cells[col + row * width]);
        }

        public bool ContainsBomb(byte data)
        {
            return ((data & ContainsBombFlag) != 0);
        }

        public int NearByBombs(int col, int row)
        {
            return NearByBombs(cells[col + row * width]);
        }

        public int NearByBombs(byte data)
        {
            return data & NearByBombsMask;
        }
    }
}