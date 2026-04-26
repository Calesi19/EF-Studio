import { CreateRecordDrawer } from "@/components/records/CreateRecordDrawer";
import { DataTable } from "@/components/table/DataTable";
import { useStudioContext } from "@/pages/StudioPage/context/StudioContext";

export function StudioWorkspace() {
  const {
    activeTab,
    tables,
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
    createDrawerOpen,
    createDraftRow,
    creatingRows,
    activeTableCreateError,
    openCreateDrawer,
    closeCreateDrawer,
    updateCreateDraftRow,
    resetCreateDraftRow,
    submitCreateRow,
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
    <>
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
        onAddRecord={openCreateDrawer}
        canAddRecord
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
      <CreateRecordDrawer
        open={createDrawerOpen}
        onOpenChange={(open) => { if (!open) closeCreateDrawer(); }}
        tableDef={selectedTable}
        draftRow={createDraftRow}
        allTables={tables}
        creating={creatingRows}
        error={activeTableCreateError}
        onChangeField={updateCreateDraftRow}
        onReset={resetCreateDraftRow}
        onSubmit={submitCreateRow}
      />
    </>
  );
}
