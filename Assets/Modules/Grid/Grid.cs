using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
	[SerializeField] private Cell _cell;
	[SerializeField] private ArcadeRigidbody _arcadeRigidbody;
	[SerializeField] private ArcadeCollider _arcadeCollider;

    [SerializeField] private int _widthCellCount = 1;
    [SerializeField] private int _heightCellCount = 1;
	[SerializeField] private float _widthMargin = 0f;
	[SerializeField] private float _heightMargin = 0f;
	[SerializeField] private Vector3 _cellSize = Vector3.one;

	private List<Cell> _cells = new List<Cell>();

	private float _currentMarginWidth, _currentMarginHeight;

	private void Start()
	{
		_currentMarginWidth = _widthMargin;
		_currentMarginHeight = _heightMargin;

		Initialize(_widthCellCount,_heightCellCount);
	}

	public void Initialize(int width, int height)
	{
        for(int x = 0; x < _widthCellCount; x++)
		{
            for(int y = 0; y < _heightCellCount; y++)
			{
				var cell = Instantiate(_cell);
				cell.transform.parent = transform;
				cell.transform.localPosition = new Vector3(x + _currentMarginWidth, y + _currentMarginHeight);
				cell.name = $"Cell-{x}:{y}";
				cell.SetSize(_cellSize);
				_cells.Add(cell);
				_currentMarginHeight += _heightMargin * 2;
			}
			_currentMarginHeight = _heightMargin;
			_currentMarginWidth += _widthMargin * 2;
		}

		_currentMarginWidth = _widthMargin;
		_currentMarginHeight = _heightMargin;
		SetCells(_arcadeCollider);
	}

	public void SetCells(CellComponent Component, bool free = true)
	{
		foreach (var cell in _cells)
		{
			if (free)
			{
				var obj = Instantiate(Component).GetComponent<Transform>();
				obj.parent = cell.transform;
				obj.localPosition = Vector3.zero;
			}
			else
			{

			}
		}

	}
}
