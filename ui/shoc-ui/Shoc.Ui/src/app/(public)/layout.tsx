import PublicNavbar from "./_components/public-navbar";

export default async function PublicLayout({ children }: any) {

    return <>
        <PublicNavbar />
        <main className="pt-16 xs:pt-20 sm:pt-24">
            {children}
        </main>
    </>
}