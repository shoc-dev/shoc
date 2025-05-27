"use client"

import { ReactNode } from "react";

export default function WorkspacePageWrapper({ header, children }: { header: ReactNode, children: ReactNode }){
    return <>
        {header}
        <div className="flex flex-col gap-4 py-4 px-6">
            {children}
        </div>
    </>
}