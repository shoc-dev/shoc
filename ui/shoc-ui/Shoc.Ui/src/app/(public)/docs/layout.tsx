import { ReactNode } from "react";

export default function DocsLayout({ children }: { children: ReactNode }) {
    return <div className="container flex mx-5 bg-card w-full h-full">
        {children}
    </div>
}