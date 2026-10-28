export interface Card {
  Id: string
  Title: string
  Description: string | null
  Tags: string[]
  Deadline: string | null // YYYY-MM-DD
  CreatedAt: string // ISO8601 UTC
}

export interface Column {
  Name: string
  Cards: Card[]
}

export interface Board {
  Name: string
  Columns: Column[]
}
