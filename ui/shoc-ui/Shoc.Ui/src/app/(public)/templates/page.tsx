import ErrorScreen from "@/components/error/error-screen";
import getIntl from "@/i18n/get-intl";
import { rpc } from "@/server-actions/rpc";
import { Metadata } from "next";
import TemplateCard from "./_components/template-card";

export const dynamic = 'force-dynamic';

export async function generateMetadata(): Promise<Metadata> {

    const intl = await getIntl();

    return {
        title: intl.formatMessage({ id: 'templates' })
    }
}

export default async function TemplatesPage() {

    const intl = await getIntl();
    const { data: templates, errors } = await rpc('template/templates/getAll')
    if (errors) {
        return <ErrorScreen errors={errors} />
    }

    return (
        <div className="max-w-screen-xl mx-auto flex flex-col gap-1 sm:min-h-[91vh] min-h-[88vh] pt-2">
            <div className="mb-7 flex flex-col gap-2">
                <h1 className="text-3xl font-extrabold">
                    {intl.formatMessage({id: 'templates'})}
                </h1>
                <p className="text-muted-foreground">
                    {intl.formatMessage({id: 'templates.headline'})}
                </p>
            </div>
            <div className="grid md:grid-cols-3 sm:grid-cols-2 grid-cols-1 sm:gap-8 gap-4 mb-5">
                {templates.map((template: any, index: number) => <TemplateCard key={index} template={template} />)}
            </div>
        </div>
    );
}

