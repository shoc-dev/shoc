"use client"

import { Skeleton } from "@/components/ui/skeleton";

export default function InvitationSkeletonCard() {
    return <div className="flex flex-col space-y-3">
    <Skeleton className="h-[150px] w-full rounded-xl" />
  </div>
}