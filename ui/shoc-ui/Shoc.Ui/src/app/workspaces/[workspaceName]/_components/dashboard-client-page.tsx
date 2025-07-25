"use client"

import DashboardJobsCard from "./dashboard-jobs-card";

export default function DashboardClientPage() {

    return <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
        <div className="grid auto-rows-min gap-4 md:grid-cols-1">
            <div className="rounded-xl bg-muted/50">
                <DashboardJobsCard />
            </div>
        </div>
    </div>
}