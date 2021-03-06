#include <limits.h>
#include <stdlib.h>
#include <string.h>

#include <Windows.h>

#include "CutScan.h"
#include "CutSegment.h"

#define MIN(X, Y) (((X) < (Y)) ? (X) : (Y))
#define MAX(X, Y) (((X) > (Y)) ? (X) : (Y))

/*
CutSegmentList** CutScan::CreateSegments(int count)
{
	CutSegmentList **s = (CutSegmentList**)malloc(count * sizeof(CutSegmentList*));

	for (int i = 0; i < count; i++)
	{
		s[i] = new CutSegmentList();
		s[i]->Clear();
	}

	return s;
}
*/

void CutScan::DeleteSegments()
{
	Clear();

	for (int i=0, k = 0; i < lineCount; i++)
		delete segments[i];

	lineCount = 0;
}

CutScan::CutScan(int lines, int angle)
{
	lineCount = lines;

	for (int i = 0; i < lines; i++)
	{
		segments[i] = new CutSegmentList();
	}

	this->angle = angle;
}

CutScan::~CutScan()
{
	Clear();

	for (int i = 0; i < lineCount; i++)
	{
		if (segments[i])
		{
			delete segments[i];
		}
	}
}

void CutScan::Clear()
{
	for (int i = 0; i < lineCount; i++) 
	{
		if (segments[i]) 
		{
			segments[i]->Clear();
		}
	}
}

void CutScan::AddSegment(int line, int start, int end)
{
	if (line >= 0 && line < lineCount) {
		if (!segments[line])
			segments[line] = new CutSegmentList();

		segments[line]->AddSegment(line, start, end);
	}
}

int CutScan::GetSegmentCount()
{
	int count = 0;
	for (int i = 0; i < lineCount; i++)
	{
		CutSegmentList *segs = segments[i];

		if (segs != nullptr)
		{
			if (segs->GetCount() > 0)
			{
				CutSegment *seg = segs->GetFirst();

				if (seg == nullptr)
					int a = 0;

				while (seg)
				{
					count++;
					seg = seg->GetNext();
				}
			}
		}		
	}

	return count;
}

int CutScan::GetLineCount()
{
	return lineCount;
}

int CutScan::GetCenter()
{
	return verticalCenter;
}

int CutScan::GetLeft()
{
	return boxLeft;
}

int CutScan::GetRight()
{
	return boxRight;
}

int CutScan::GetTop()
{
	return boxTop;
}

int CutScan::GetBottom()
{
	return boxBottom;
}

void CutScan::Normalize(int w, int h)
{
	int minx = LONG_MAX;
	int miny = LONG_MAX;
	int maxx = LONG_MIN;
	int maxy = LONG_MIN;

	int c = w / 2;
	int segLine = -c;

	firstLine = LONG_MAX;
	lastLine = LONG_MIN;

	for (int i = 0; i < lineCount; i++)
	{
		int segCount = segments[i]->GetCount();
		CutSegment *seg = segments[i]->GetFirst();

		if (seg)
		{
			if (segLine < firstLine)
			{
				firstLine = segLine;
			}

			if (segLine > lastLine)
			{
				lastLine = segLine;
			}

			do
			{
				minx = MIN(seg->GetStart(), minx);
				maxx = MAX(seg->GetEnd(), maxx);

				seg->Normalize(0);
				seg->SetLine(segLine);
			} while (seg = seg->GetNext());
		}

		if (segCount > 0)
		{
			miny = MIN(i, miny);
			maxy = MAX(i, maxy);
		}

		segLine++;
	}

	int tl = maxy - miny;

	if (miny > 0)
	{
		for (int s = miny, d = 0; s < lineCount; s++, d++)
		{
			CutSegmentList* tmp = segments[d];
			segments[d] = segments[s];
			segments[s] = tmp;
		}
	}

	lineCount = tl;
	this->verticalCenter = lineCount / 2;
	this->horizontalCenter = w / 2;

	if (startPos != nullptr)
		delete[] startPos;

	minx = LONG_MAX;
	maxx = LONG_MIN;
	miny = LONG_MAX;
	maxy = LONG_MIN;

	startPos = new int[lineCount + 1];

	for (int i = 0; i < lineCount; i++)
	{
		if (segments[i]->GetCount() > 0) 		
			startPos[i] = segments[i]->GetFirst()->GetStart();		
		else		
			startPos[i] = INT_MAX;		

		if (CutSegment *seg = segments[i]->GetFirst())
		{
			int line = seg->GetLine();

			int lineMin = LONG_MAX;
			int lineMax = LONG_MIN;

			do
			{
				minx = MIN(minx, seg->GetStart());
				maxx = MAX(maxx, seg->GetEnd());
				miny = MIN(miny, line);
				maxy = MAX(maxy, line);

				lineMax = MAX(lineMax, seg->GetEnd());
				lineMin = MIN(lineMin, seg->GetStart());

			} while (seg = seg->GetNext());

			if (line == 0)
			{
				middleMin = lineMin;
				middleMax = lineMax;
				middleLen = lineMax - lineMin;
			}
		}
	}

	startPos[lineCount] = INT_MAX;
	
	boxLeft = minx;
	boxRight = maxx;
	boxTop = miny;
	boxBottom = maxy;

	width = boxRight - boxLeft;
}

int CutScan::GetMiddleMin()
{
	return middleMin;
}

int CutScan::GetMiddleMax()
{
	return middleMax;
}

int CutScan::GetMiddleLen()
{
	return middleLen;
}

int CutScan::GetStartLine(int y)
{
	return firstLine + y;
}

int CutScan::GetLastLine(int y)
{
	return lastLine + y;
}

bool CutScan::TestLimits(int x, int y, int top, int left, int bottom)
{
	if (y + boxTop < top)
		return false;

	if (x + boxLeft < left)
		return false;

	if (y + boxBottom > bottom)
		return false;

	return true;
}

int CutScan::GetWidth()
{
	return width;
}

CutSegmentList** CutScan::GetSegments() 
{
	return segments;
}

void CutScan::ScanLineMap(float w, float h, LineList *map, int mapSize)
{
	Clear();

	this->width = width;
	this->verticalCenter = (int)width / 2;

	float cross[1024];

	int h2 = (int)h / 2;

	for (int i = 0; i < h; i++)
	{
		int mapIndex = i / mapSize;		
		int count = map[mapIndex].FillCrossPoints(cross, 1024, i-h2);

		if (count > 0)
		{
			if (count >= 2)
			{
				for (int k = 0; k < count; k += 2)
				{
					AddSegment(i, (int)cross[k], (int)cross[k + 1]);
				}
			}
			else
			{
				AddSegment(i, (int)cross[0], (int)cross[0] + 1);
			}			
		}
	}

	Normalize((int)w, (int)h);
}

void CutScan::ScanImageData(int width, int height, void* data)
{
	Clear();

	this->width = width;
	verticalCenter = width >> 1;

	int* p = (int*)data;

	int s = 0, e = 0;
	int line = 0;
	int len = width + height;

	bool reading = false;
	
	for (int y = 0; y < height; y++)
	{
		for (int x = 0; x < width; x++)
		{
			int index = y * width + x;

			if (!reading)
			{
				if (p[index] != 0)
				{
					reading = true;
					line = y;
					e = s = x;
				}
			}
			else
			{
				if (p[index] != 0) 
				{
					e++;
				}
				else
				{
					reading = false;
					AddSegment(line, s, e);
				}				
			}
		}

		if (reading)
		{
			reading = false;
			AddSegment(line, s, e);
		}
	}

	Normalize(width, height);
}