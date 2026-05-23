import { Card, Placeholder } from 'react-bootstrap'

interface TableSkeletonProps {
  rows?: number
  cols?: number
}

export function TableSkeleton({ rows = 5, cols = 4 }: TableSkeletonProps) {
  return (
    <Card className="border-0 shadow-sm">
      <Card.Body>
        <Placeholder as="div" animation="glow">
          {Array.from({ length: rows }).map((_, rowIdx) => (
            <div key={rowIdx} className="d-flex gap-3 mb-3">
              {Array.from({ length: cols }).map((_, colIdx) => (
                <Placeholder
                  key={colIdx}
                  xs={Math.floor(12 / cols)}
                  className="rounded"
                  bg="secondary"
                />
              ))}
            </div>
          ))}
        </Placeholder>
      </Card.Body>
    </Card>
  )
}

