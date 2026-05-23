import { Pagination as BsPagination } from 'react-bootstrap'

interface PaginationProps {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
}

export function Pagination({ currentPage, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null

  const pages: number[] = []
  const start = Math.max(1, currentPage - 2)
  const end = Math.min(totalPages, currentPage + 2)

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }

  return (
    <BsPagination size="sm" className="mb-0 flex-wrap">
      <BsPagination.First onClick={() => onPageChange(1)} disabled={currentPage === 1} />
      <BsPagination.Prev onClick={() => onPageChange(currentPage - 1)} disabled={currentPage === 1} />
      {start > 1 && <BsPagination.Ellipsis disabled />}
      {pages.map((p) => (
        <BsPagination.Item key={p} active={p === currentPage} onClick={() => onPageChange(p)}>
          {p}
        </BsPagination.Item>
      ))}
      {end < totalPages && <BsPagination.Ellipsis disabled />}
      <BsPagination.Next onClick={() => onPageChange(currentPage + 1)} disabled={currentPage === totalPages} />
      <BsPagination.Last onClick={() => onPageChange(totalPages)} disabled={currentPage === totalPages} />
    </BsPagination>
  )
}

