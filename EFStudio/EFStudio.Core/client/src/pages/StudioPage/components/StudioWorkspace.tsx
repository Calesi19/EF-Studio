import { DataTable } from "@/components/table/DataTable";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";

export function StudioWorkspace() {
  const {
    activeTab,
    selectedTable,
    currentRows,
    currentTotalPages,
    currentTotalRows,
    jumpToReference,
    changeFilter,
    changePage,
    changePageSize,
    changeSort,
  } = useStudioContext();

  if (!activeTab || !selectedTable) {
    return null;
  }

  return (
    <DataTable
      key={activeTab.id}
      columns={selectedTable.columns}
      rows={currentRows}
      totalRows={currentTotalRows}
      totalPages={currentTotalPages}
      filter={activeTab.filter}
      sort={activeTab.sort}
      pagination={activeTab.pagination}
      onFilterChange={changeFilter}
      onSortChange={changeSort}
      onPageChange={changePage}
      onPageSizeChange={changePageSize}
      onAddRecord={() => {}}
      canAddRecord={false}
      onEditRecord={() => {}}
      onDeleteRecord={() => {}}
      onBulkDelete={() => {}}
      onJumpToRef={jumpToReference}
      readOnly
    />
  );
}
