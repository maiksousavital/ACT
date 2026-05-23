import { Pagination as BsPagination } from 'react-bootstrap'

interface PaginationProps {
  currentPage: number
  totalPages: number
  totalCount?: number
  pageSize?: number
  onPageChange: (page: number) => void
}

export function Pagination({ currentPage, totalPages, totalCount, pageSize, onPageChange }: PaginationProps) {
  // Compute totalPages from totalCount/pageSize if totalPages is falsy (backend might not return it)
  const effectiveTotalPages = totalPages || (totalCount && pageSize ? Math.ceil(totalCount / pageSize) : 1)

  if (effectiveTotalPages <= 1) return null

  const pages: number[] = []
  const start = Math.max(1, currentPage - 2)
  const end = Math.min(effectiveTotalPages, currentPage + 2)

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }

  return (
    <BsPagination size="sm" className="mb-0 flex-wrap">
      <BsPagination.First onClick={() => currentPage > 1 && onPageChange(1)} disabled={currentPage <= 1} />
      <BsPagination.Prev onClick={() => currentPage > 1 && onPageChange(currentPage - 1)} disabled={currentPage <= 1} />
      {start > 1 && <BsPagination.Ellipsis disabled />}
      {pages.map((p) => (
        <BsPagination.Item key={p} active={p === currentPage} onClick={() => p !== currentPage && onPageChange(p)}>
          {p}
        </BsPagination.Item>
      ))}
      {end < effectiveTotalPages && <BsPagination.Ellipsis disabled />}
      <BsPagination.Next onClick={() => currentPage < effectiveTotalPages && onPageChange(currentPage + 1)} disabled={currentPage >= effectiveTotalPages} />
      <BsPagination.Last onClick={() => currentPage < effectiveTotalPages && onPageChange(effectiveTotalPages)} disabled={currentPage >= effectiveTotalPages} />
    </BsPagination>
  )
}

