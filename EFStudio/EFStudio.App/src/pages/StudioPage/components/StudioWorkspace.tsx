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
    deleteRows,
    deleteSelectionResetKey,
    pendingEdits,
    savingEdits,
    setCellEdit,
    saveEdits,
    discardEdits,
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
      selectionResetKey={deleteSelectionResetKey}
      onEditRecord={() => {}}
      onDeleteRecord={() => {}}
      onBulkDelete={deleteRows}
      onJumpToRef={jumpToReference}
      pendingEdits={pendingEdits}
      pendingEditCount={pendingEdits.size}
      savingEdits={savingEdits}
      onCellEdit={setCellEdit}
      onSaveEdits={saveEdits}
      onDiscardEdits={discardEdits}
    />
  );
}
