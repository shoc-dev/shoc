"use client"

import { Badge } from "@/components/ui/badge"
import { Separator } from "@/components/ui/separator"
import Link from "next/link"

interface TemplateProps {
    name: string,
    title: string,
    description: string,
    author: string,
    variants: string[]
}

export default function TemplateCard({ template: { name, title, description, author, variants } }: { template: TemplateProps }) {

    return <div
        className="flex flex-col gap-2 items-start border rounded-md py-5 px-3"
    >
        <h3 className="text-md font-semibold -mt-1 pr-7">
            <Link href={`/templates/${name}/variants/default`} prefetch={false}>
                {title}
            </Link>
        </h3>
        <p className="text-sm text-muted-foreground">{description} </p>
        <Separator />
        <div className="flex space-x-2 w-full mt-auto">
            {variants.map(variant => <Link key={variant} prefetch={false} href={`/templates/${name}/variants/${variant}`}>
                <Badge variant="outline">
                    {variant}
                </Badge>
            </Link>)}
        </div>
    </div>
}