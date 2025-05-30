"use client"

import { useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { useIntl } from "react-intl";
import NoInvitations from "./no-invitations";
import InvitationSkeletonCard from "./invitation-skeleton-card";
import InvitationCard from "./invitation-card";

const DEFAULT_PAGE_SIZE = 12;
const DEFAULT_SKELETON_PAGE_SIZE = 6;

export default function InvitationsCardList(
  { progress, items = [], onUpdate = () => { } }:
    { progress?: boolean, items?: any[], onUpdate: () => void }) {

  const [page, setPage] = useState<number>(0);
  const intl = useIntl();

  const current = useMemo(() => {
    return items.slice(page * DEFAULT_PAGE_SIZE, Math.max((page + 1) * DEFAULT_PAGE_SIZE, items.length - 1))
  }, [page, items])

  if (progress) {
    return <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 4 gap-4">
      {Array.from(Array(DEFAULT_SKELETON_PAGE_SIZE).keys()).map(idx => <InvitationSkeletonCard key={idx} />)}
    </div>
  }
  if (items.length === 0) {
    return <NoInvitations />
  }

  return <div className="flex flex-col space-y-4">
    <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 4 gap-4">
      {current.map((item: any) => <InvitationCard key={item.id} invitation={item} onDeclined={() => onUpdate()} onAccepted={() => onUpdate()} />)}
    </div>
    {items.length > DEFAULT_PAGE_SIZE && <div className="flex mx-auto space-x-2">
      <Button variant="outline" disabled={page === 0} onClick={() => setPage(prev => prev - 1)}>
        <ChevronLeft className="mr-2 w-4 h-4" />
        {intl.formatMessage({ id: 'global.navigation.prev' })}
      </Button>
      <Button variant="outline" disabled={page + 1 > items.length / DEFAULT_PAGE_SIZE} onClick={() => setPage(prev => prev + 1)}>
        {intl.formatMessage({ id: 'global.navigation.next' })}
        <ChevronRight className="ml-2 w-4 h-4" />
      </Button>
    </div>}
  </div>

}